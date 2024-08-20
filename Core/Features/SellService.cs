using System;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Shared;
using Microsoft.EntityFrameworkCore;

namespace Core.Features
{
    internal class SellService : ISellService
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

            // Calculate the average price per share
            var averagePricePerShare = stockHolding.TotalPurchasePrice / stockHolding.Quantity;

            // Calculate the total purchase price corresponding to the quantity being sold
            var reductionInPurchasePrice = averagePricePerShare * quantity;

            // Reduce the quantity and adjust the total purchase price
            stockHolding.Quantity -= quantity;
            stockHolding.TotalPurchasePrice -= reductionInPurchasePrice;

            // Remove the stock holding if the quantity is now zero
            if (stockHolding.Quantity == 0)
            {
                _context.StockHoldings.Remove(stockHolding);
            }

            // Increase the user's balance by the total proceeds
            var totalProceeds = price * quantity;
            user.Balance += totalProceeds;

            // Record the stock holding item (a record of the sale)
            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = stockSymbol,
                Quantity = quantity,
                UnitPrice = price,
                Type = StockHoldingItemType.Sale,
                OrderProcessId = orderProcessId == default ? null : orderProcessId
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            // Record the transaction
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
