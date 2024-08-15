using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Data.Entity.User;
using Core.Shared;
using Microsoft.EntityFrameworkCore;

namespace Core.Features
{
    public class BuyService : IBuyService
    {
        private readonly ApplicationDbContext _context;

        public BuyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> BuyStockAsync(AppUser user, string stockSymbol, int quantity, decimal price, Guid orderProcessId = default)
        {
            var totalCost = price * quantity;

            if (user.Balance < totalCost)
            {
                return Result.Failure(new Error("InsufficientFunds", "User does not have sufficient balance."));
            }

            user.Balance -= totalCost;

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.StockSymbol == stockSymbol);

            if (stockHolding == null)
            {
                stockHolding = new StockHolding
                {
                    UserId = user.Id,
                    StockSymbol = stockSymbol,
                    Quantity = quantity,
                    TotalPurchasePrice = totalCost
                };
                _context.StockHoldings.Add(stockHolding);
            }
            else
            {
                stockHolding.Quantity += quantity;
                stockHolding.TotalPurchasePrice += totalCost;
            }

            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = stockSymbol,
                Quantity = quantity,
                UnitPrice = price,
                Type = StockHoldingItemType.Purchase,
                OrderProcessId = orderProcessId
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = -totalCost,
                Type = TransActionType.Negative,
                Description = $"Purchased {quantity} shares of {stockSymbol} at {price:C} per share",
                StockHoldingItem = stockHoldingItem,
                CreatedDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}