using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Features.ShowPortfolio;
using Core.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Core.Service.StockApi;
using System.Collections.Generic;
using Core.Data.Entity;

namespace Core.Features.ShowPortfolio
{
    internal class ShowPortfolioHandler : IRequestHandler<ShowPortfolioCommand, Result<ShowPortfolioResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;

        public ShowPortfolioHandler(ApplicationDbContext context, IStockApiService stockApiService)
        {
            _context = context;
            _stockApiService = stockApiService;
        }

        public async Task<Result<ShowPortfolioResponse>> Handle(ShowPortfolioCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.StockHoldings)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<ShowPortfolioResponse>(new Error("UserNotFound", "User not found."));
            }

            var totalPendingAmount = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.OrderProcess.Status == OrderProcessStatus.Pending)
                .SumAsync(o => o.Quantity * o.TargetPrice, cancellationToken);

            var totalBalance = user.Balance;
            var availableBalance = user.Balance - totalPendingAmount;

            var stockSymbols = user.StockHoldings
                .Select(sh => sh.StockSymbol)
                .Distinct()
                .ToList();

            var stockPrices = new Dictionary<string, decimal>();
            foreach (var symbol in stockSymbols)
            {
                var currentPrice = await _stockApiService.GetStockPriceAsync(symbol);
                stockPrices[symbol] = currentPrice;
            }

            decimal totalPortfolioValue = 0;
            decimal totalChange = 0;

            var stockHoldingItems = new List<ShowPortfolioResponse.StockHoldingItemDetail>();

            foreach (var stockHolding in user.StockHoldings)
            {
                var currentPrice = stockPrices[stockHolding.StockSymbol];

                var holdingValue = currentPrice * stockHolding.Quantity;
                var initialValue = stockHolding.TotalPurchasePrice;
                var change = holdingValue - initialValue;

                totalPortfolioValue += holdingValue;
                totalChange += change;

                stockHoldingItems.Add(new ShowPortfolioResponse.StockHoldingItemDetail
                {
                    StockSymbol = stockHolding.StockSymbol,
                    Quantity = stockHolding.Quantity,
                    UnitPrice = currentPrice
                });
            }

            return Result.Success(new ShowPortfolioResponse
            {
                IsSuccess = true,
                Message = "Portfolio retrieved successfully.",
                StockHoldingItems = stockHoldingItems,
                Change = totalChange,
                TotalPortfolioValue = totalPortfolioValue,
                TotalBalance = totalBalance,
                AvailableBalance = availableBalance
            });
        }
    }
}
