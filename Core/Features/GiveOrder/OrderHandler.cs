using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Service.StockApi;
using Core.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.Features.GiveOrder
{
    public class OrderHandler : IRequestHandler<OrderCommand, Result<OrderResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IValidator<OrderCommand> _validator;

        public OrderHandler(ApplicationDbContext context, IStockApiService stockApiService, IValidator<OrderCommand> validator)
        {
            _context = context;
            _stockApiService = stockApiService;
            _validator = validator;
        }

        public async Task<Result<OrderResponse>> Handle(OrderCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result.Failure<OrderResponse>(new Error("ValidationFailed", string.Join(", ", errors)));
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<OrderResponse>(new Error("UserNotFound", "User not found."));
            }

            if (request.OrderType == OrderType.Buy)
            {
                var totalPrice = request.TargetPrice * request.Quantity;
                if (user.Balance < totalPrice)
                {
                    return Result.Failure<OrderResponse>(new Error("InsufficientFunds", "Insufficient funds: You need at least " + totalPrice.ToString("C") + " to place this buy order."));
                }
            }

            if (request.OrderType == OrderType.Sell)
            {
                var stockHolding = await _context.StockHoldings
                    .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == request.StockSymbol);

                if (stockHolding == null || stockHolding.Quantity < request.Quantity)
                {
                    return Result.Failure<OrderResponse>(new Error("InsufficientHoldings", "Insufficient holdings: You do not have enough shares to place this sell order."));
                }

                var activeSellOrders = await _context.OrderProcesses
                    .Where(op => op.Order.UserId == user.Id && op.Order.StockSymbol == request.StockSymbol && op.Order.OrderType == OrderType.Sell && op.Status == OrderProcessStatus.Pending)
                    .SumAsync(op => op.Order.Quantity);

                if (activeSellOrders + request.Quantity > stockHolding.Quantity)
                {
                    return Result.Failure<OrderResponse>(new Error("InvalidOrder", "The total quantity of active sell orders exceeds the available holdings."));
                }
            }

            var order = new Order
            {
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                TargetPrice = request.TargetPrice,
                CreatedBy = user.UserName,
                CreatedDate = DateTime.UtcNow,
                OrderType = request.OrderType,
                UserId = user.Id,
                User = user
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            var orderProcess = new OrderProcess
            {
                Order = order,
                Status = OrderProcessStatus.Pending
            };
            _context.OrderProcesses.Add(orderProcess);
            await _context.SaveChangesAsync(cancellationToken);

            // Get the current stock price
            var stockPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);

            string message;
            if (request.OrderType == OrderType.Buy && stockPrice <= request.TargetPrice)
            {
                var result = await ExecuteBuyOrder(user, order, stockPrice, orderProcess.Id);
                if (!result.IsSuccess)
                {
                    return Result.Failure<OrderResponse>(result.Error);
                }
                orderProcess.Status = OrderProcessStatus.Completed;
                message = $"Buy order completed: Purchased {order.Quantity} shares of {order.StockSymbol} at {stockPrice:C} per share.";
            }
            else if (request.OrderType == OrderType.Sell && stockPrice >= request.TargetPrice)
            {
                var result = await ExecuteSellOrder(user, order, stockPrice, orderProcess.Id);
                if (!result.IsSuccess)
                {
                    return Result.Failure<OrderResponse>(result.Error);
                }
                orderProcess.Status = OrderProcessStatus.Completed;
                message = $"Sell order completed: Sold {order.Quantity} shares of {order.StockSymbol} at {stockPrice:C} per share.";
            }
            else
            {
                message = $"Order placed successfully and is pending: {order.OrderType} {order.Quantity} shares of {order.StockSymbol} at a target price of {order.TargetPrice:C}.";
            
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new OrderResponse
            {
                IsSuccess = true,
                Message = message,
                OrderId = order.Id
            });
        }

        private async Task<Result> ExecuteBuyOrder(AppUser user, Order order, decimal stockPrice, Guid orderProcessId)
        {
            var totalPrice = stockPrice * order.Quantity;
            if (user.Balance < totalPrice)
            {
                return Result.Failure(new Error("InsufficientFunds", "User does not have sufficient balance."));
            }

            user.Balance -= totalPrice;

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == order.StockSymbol);

            if (stockHolding == null)
            {
                stockHolding = new StockHolding
                {
                    UserId = user.Id,
                    StockSymbol = order.StockSymbol,
                    Quantity = order.Quantity,
                    TotalPurchasePrice = totalPrice
                };
                _context.StockHoldings.Add(stockHolding);
            }
            else
            {
                stockHolding.Quantity += order.Quantity;
                stockHolding.TotalPurchasePrice += totalPrice;
            }

            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = order.StockSymbol,
                Quantity = order.Quantity,
                UnitPrice = stockPrice,
                Type = StockHoldingItemType.Purchase,
                OrderProcessId = orderProcessId
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = -totalPrice,
                Type = TransActionType.Negative,
                Description = $"Purchased {order.Quantity} shares of {order.StockSymbol} at {stockPrice} per share",
                StockHoldingItem = stockHoldingItem,
                CreatedDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        private async Task<Result> ExecuteSellOrder(AppUser user, Order order, decimal stockPrice, Guid orderProcessId)
        {
            var totalPrice = stockPrice * order.Quantity;

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == order.StockSymbol);

            if (stockHolding == null || stockHolding.Quantity < order.Quantity)
            {
                return Result.Failure(new Error("InsufficientHoldings", "User does not have enough stock holdings to sell."));
            }

            stockHolding.Quantity -= order.Quantity;

            if (stockHolding.Quantity == 0)
            {
                _context.StockHoldings.Remove(stockHolding);
            }
            else
            {
                stockHolding.TotalPurchasePrice -= totalPrice;
            }

            user.Balance += totalPrice;

            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = order.StockSymbol,
                Quantity = order.Quantity,
                UnitPrice = stockPrice,
                Type = StockHoldingItemType.Sale,
                OrderProcessId = orderProcessId
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = totalPrice,
                Type = TransActionType.Positive,
                Description = $"Sold {order.Quantity} shares of {order.StockSymbol} at {stockPrice} per share",
                StockHoldingItem = stockHoldingItem,
                CreatedDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Result.Success();
        }

    }
}
