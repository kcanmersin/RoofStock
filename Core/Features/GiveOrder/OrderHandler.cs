using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Service.StockApi;
using Core.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.GiveOrder
{
    internal class OrderHandler : IRequestHandler<OrderCommand, Result<OrderResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IBuyService _buyService;
        private readonly ISellService _sellService;
        private readonly IValidator<OrderCommand> _validator;

        public OrderHandler(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            IBuyService buyService,
            ISellService sellService,
            IValidator<OrderCommand> validator)
        {
            _context = context;
            _stockApiService = stockApiService;
            _buyService = buyService;
            _sellService = sellService;
            _validator = validator;
        }

        public async Task<Result<OrderResponse>> Handle(OrderCommand request, CancellationToken cancellationToken)
        {
            // Validation
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

            var stockPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);

            var pendingBuyOrders = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.OrderType == OrderType.Buy && o.OrderProcess.Status == OrderProcessStatus.Pending)
                .ToListAsync();

            var reservedBalance = pendingBuyOrders.Sum(o => o.Quantity * o.TargetPrice);
            var availableBalance = user.Balance - reservedBalance;

            var pendingSellOrders = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.OrderType == OrderType.Sell && o.OrderProcess.Status == OrderProcessStatus.Pending)
                .ToListAsync();

            var reservedHoldings = pendingSellOrders
                .Where(o => o.StockSymbol == request.StockSymbol)
                .Sum(o => o.Quantity);

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == request.UserId && sh.StockSymbol == request.StockSymbol);

            var availableHoldings = stockHolding != null ? stockHolding.Quantity - reservedHoldings : 0;

            if (request.OrderType == OrderType.Buy)
            {
                var totalCost = request.Quantity * stockPrice;
                if (totalCost > availableBalance)
                {
                    return Result.Failure<OrderResponse>(new Error("InsufficientFunds", "Insufficient funds: You do not have enough available balance to place this order."));
                }
            }
            else if (request.OrderType == OrderType.Sell)
            {
                if (request.Quantity > availableHoldings)
                {
                    return Result.Failure<OrderResponse>(new Error("InsufficientHoldings", "Insufficient holdings: You do not have enough available shares to place this order."));
                }
            }

            // If the price conditions are met, execute the order immediately
            if (request.OrderType == OrderType.Buy && stockPrice <= request.TargetPrice)
            {
                var result = await _buyService.BuyStockAsync(user, request.StockSymbol, request.Quantity, stockPrice);
                if (!result.IsSuccess)
                {
                    return Result.Failure<OrderResponse>(result.Error);
                }
                return Result.Success(new OrderResponse
                {
                    IsSuccess = true,
                    Message = $"Buy order completed: Purchased {request.Quantity} shares of {request.StockSymbol} at {stockPrice:C} per share.",
                    OrderId = Guid.NewGuid() 
                });
            }
            else if (request.OrderType == OrderType.Sell && stockPrice >= request.TargetPrice)
            {
                var result = await _sellService.SellStockAsync(user, request.StockSymbol, request.Quantity, stockPrice);
                if (!result.IsSuccess)
                {
                    return Result.Failure<OrderResponse>(result.Error);
                }
                return Result.Success(new OrderResponse
                {
                    IsSuccess = true,
                    Message = $"Sell order completed: Sold {request.Quantity} shares of {request.StockSymbol} at {stockPrice:C} per share.",
                    OrderId = Guid.NewGuid() 
                });
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

            return Result.Success(new OrderResponse
            {
                IsSuccess = true,
                Message = $"Order placed successfully and is pending: {order.OrderType} {order.Quantity} shares of {order.StockSymbol} at a target price of {order.TargetPrice:C}.",
                OrderId = order.Id
            });
        }
    }
}
