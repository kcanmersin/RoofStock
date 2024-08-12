using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Service.StockApi;
using Microsoft.EntityFrameworkCore;

namespace Core.Service.OrderBackgroundService
{
    public class OrderBackgroundService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;

        public OrderBackgroundService(ApplicationDbContext context, IStockApiService stockApiService)
        {
            _context = context;
            _stockApiService = stockApiService;
        }

        public async Task CheckAndProcessOrders()
        {
            var pendingOrders = await _context.OrderProcesses
                .Include(op => op.Order)
                .Where(op => op.Status == OrderProcessStatus.Pending)
                .ToListAsync();

            foreach (var orderProcess in pendingOrders)
            {
                var currentPrice = await _stockApiService.GetStockPriceAsync(orderProcess.Order.StockSymbol);

                if (orderProcess.Order.OrderType == OrderType.Buy && currentPrice <= orderProcess.Order.TargetPrice)
                {
                    await ExecuteBuyOrder(orderProcess, currentPrice);
                }
                else if (orderProcess.Order.OrderType == OrderType.Sell && currentPrice >= orderProcess.Order.TargetPrice)
                {
                    await ExecuteSellOrder(orderProcess, currentPrice);
                }
            }
        }

        private async Task ExecuteBuyOrder(OrderProcess orderProcess, decimal currentPrice)
        {
            var user = await _context.Users.FindAsync(orderProcess.Order.UserId);
            var totalPrice = currentPrice * orderProcess.Order.Quantity;

            if (user == null || user.Balance < totalPrice)
            {
                orderProcess.Status = OrderProcessStatus.Failed;
            }
            else
            {
                // Bakiyeden toplam fiyat düşülüyor
                user.Balance -= totalPrice;

                var stockHolding = await _context.StockHoldings
                    .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == orderProcess.Order.StockSymbol);

                if (stockHolding == null)
                {
                    stockHolding = new StockHolding
                    {
                        UserId = orderProcess.Order.UserId,
                        StockSymbol = orderProcess.Order.StockSymbol,
                        Quantity = orderProcess.Order.Quantity,
                        TotalPurchasePrice = totalPrice
                    };
                    _context.StockHoldings.Add(stockHolding);
                }
                else
                {
                    stockHolding.Quantity += orderProcess.Order.Quantity;
                    stockHolding.TotalPurchasePrice += totalPrice;
                }

                var stockHoldingItem = new StockHoldingItem
                {
                    StockSymbol = orderProcess.Order.StockSymbol,
                    Quantity = orderProcess.Order.Quantity,
                    UnitPrice = currentPrice,
                    Type = StockHoldingItemType.Purchase,
                    OrderProcessId = orderProcess.Id
                };
                _context.StockHoldingItems.Add(stockHoldingItem);

                var transaction = new Transaction
                {
                    UserId = user.Id,
                    Amount = -totalPrice,
                    Type = TransActionType.Negative,
                    Description = $"Purchased {orderProcess.Order.Quantity} shares of {orderProcess.Order.StockSymbol} at {currentPrice} per share",
                    StockHoldingItem = stockHoldingItem,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);

                orderProcess.Status = OrderProcessStatus.Completed;
            }

            await _context.SaveChangesAsync();
        }

        private async Task ExecuteSellOrder(OrderProcess orderProcess, decimal currentPrice)
        {
            var user = await _context.Users.FindAsync(orderProcess.Order.UserId);
            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == orderProcess.Order.StockSymbol);

            if (user == null || stockHolding == null || stockHolding.Quantity < orderProcess.Order.Quantity)
            {
                orderProcess.Status = OrderProcessStatus.Failed;
            }
            else
            {
                var totalPrice = currentPrice * orderProcess.Order.Quantity;

                // Hisselerden istenilen miktar düşürülüyor
                stockHolding.Quantity -= orderProcess.Order.Quantity;

                if (stockHolding.Quantity == 0)
                {
                    _context.StockHoldings.Remove(stockHolding);
                }

                // Satıştan elde edilen gelir bakiyeye ekleniyor
                user.Balance += totalPrice;

                var stockHoldingItem = new StockHoldingItem
                {
                    StockSymbol = orderProcess.Order.StockSymbol,
                    Quantity = orderProcess.Order.Quantity,
                    UnitPrice = currentPrice,
                    Type = StockHoldingItemType.Sale,
                    OrderProcessId = orderProcess.Id
                };
                _context.StockHoldingItems.Add(stockHoldingItem);

                var transaction = new Transaction
                {
                    UserId = user.Id,
                    Amount = totalPrice,
                    Type = TransActionType.Positive,
                    Description = $"Sold {orderProcess.Order.Quantity} shares of {orderProcess.Order.StockSymbol} at {currentPrice} per share",
                    StockHoldingItem = stockHoldingItem,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);

                orderProcess.Status = OrderProcessStatus.Completed;
            }

            await _context.SaveChangesAsync();
        }
    }
}
