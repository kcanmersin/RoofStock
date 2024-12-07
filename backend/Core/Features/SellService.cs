using System;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Shared;
using Microsoft.EntityFrameworkCore;

namespace Core.Features
{
    public class SellService : ISellService
    {
        private readonly ApplicationDbContext _context;

        public SellService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> SellStockAsync(AppUser user, string stockSymbol, int quantity, decimal price, Guid orderProcessId = default)
        {
            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == stockSymbol);

            if (stockHolding == null || stockHolding.Quantity < quantity)
            {
                return Result.Failure(new Error("InsufficientHoldings", "User does not have enough stock holdings to sell."));
            }

            var averagePricePerShare = stockHolding.TotalPurchasePrice / stockHolding.Quantity;

            var reductionInPurchasePrice = averagePricePerShare * quantity;

            stockHolding.Quantity -= quantity;
            stockHolding.TotalPurchasePrice -= reductionInPurchasePrice;

            if (stockHolding.Quantity == 0)
            {
                _context.StockHoldings.Remove(stockHolding);
            }

            var totalProceeds = price * quantity;
            user.Balance += totalProceeds;

            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = stockSymbol,
                Quantity = quantity,
                UnitPrice = price,
                Type = StockHoldingItemType.Sale,
                OrderProcessId = orderProcessId == default ? null : orderProcessId
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = totalProceeds,
                Type = TransActionType.Positive,
                Description = $"Sold {quantity} shares of {stockSymbol} at {price:C} per share",
                StockHoldingItem = stockHoldingItem,
                CreatedDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
