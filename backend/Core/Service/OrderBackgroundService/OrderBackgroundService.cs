using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Features;
using Core.Service.StockApi;
using Microsoft.EntityFrameworkCore;

namespace Core.Service.OrderBackgroundService
{
    public class OrderBackgroundService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IBuyService _buyService;
        private readonly ISellService _sellService;

        public OrderBackgroundService(ApplicationDbContext context, IStockApiService stockApiService, IBuyService buyService, ISellService sellService)
        {
            _context = context;
            _stockApiService = stockApiService;
            _buyService = buyService;
            _sellService = sellService;
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
            if (user == null)
            {
                orderProcess.Status = OrderProcessStatus.Failed;
            }
            else
            {
                var totalCost = currentPrice * orderProcess.Order.Quantity;

                var pendingBuyOrders = await _context.Orders
                    .Where(o => o.UserId == user.Id && o.OrderType == OrderType.Buy && o.OrderProcess.Status == OrderProcessStatus.Pending)
                    .ToListAsync();

                var reservedBalance = pendingBuyOrders.Sum(o => o.Quantity * o.TargetPrice);
                var availableBalance = user.Balance - reservedBalance;

                if (availableBalance >= totalCost)
                {
                    var result = await _buyService.BuyStockAsync(user, orderProcess.Order.StockSymbol, orderProcess.Order.Quantity, currentPrice);
                    if (!result.IsSuccess)
                    {
                        orderProcess.Status = OrderProcessStatus.Failed;
                    }
                    else
                    {
                        orderProcess.Status = OrderProcessStatus.Completed;
                    }
                }
                else
                {
                    orderProcess.Status = OrderProcessStatus.Failed;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task ExecuteSellOrder(OrderProcess orderProcess, decimal currentPrice)
        {
            var user = await _context.Users.FindAsync(orderProcess.Order.UserId);
            if (user == null)
            {
                orderProcess.Status = OrderProcessStatus.Failed;
            }
            else
            {
                var stockHolding = await _context.StockHoldings
                    .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == orderProcess.Order.StockSymbol);

                var pendingSellOrders = await _context.Orders
                    .Where(o => o.UserId == user.Id && o.OrderType == OrderType.Sell && o.OrderProcess.Status == OrderProcessStatus.Pending)
                    .ToListAsync();

                var reservedHoldings = pendingSellOrders.Sum(o => o.Quantity);
                var availableHoldings = stockHolding != null ? stockHolding.Quantity - reservedHoldings : 0;

                if (availableHoldings >= orderProcess.Order.Quantity)
                {
                    var result = await _sellService.SellStockAsync(user, orderProcess.Order.StockSymbol, orderProcess.Order.Quantity, currentPrice);
                    if (!result.IsSuccess)
                    {
                        orderProcess.Status = OrderProcessStatus.Failed;
                    }
                    else
                    {
                        orderProcess.Status = OrderProcessStatus.Completed;
                    }
                }
                else
                {
                    orderProcess.Status = OrderProcessStatus.Failed;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
