using Core.Data.Entity;
using Core.Data;
using Core.Service.StockApi;
using Core.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioHandler : IRequestHandler<ShowPortfolioCommand, Result<ShowPortfolioResponse>>
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
                    UnitPrice = currentPrice,
                    HoldingValue = holdingValue,
                    Change = change
                });
            }

            var userTransactions = await _context.Transactions
                .Where(t => t.UserId == request.UserId)
                .OrderByDescending(t => t.CreatedDate) 
                .ToListAsync(cancellationToken);

            var transactionDetails = userTransactions.Select(t => new ShowPortfolioResponse.TransactionDetail
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(), 
                Description = t.Description,
                CreatedDate = t.CreatedDate
            }).ToList();

            var userOrders = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.OrderProcess.Status != OrderProcessStatus.Canceled)
                .Include(o => o.OrderProcess)
                .ToListAsync(cancellationToken);

            var orderDetails = userOrders.Select(o => new ShowPortfolioResponse.OrderDetail
            {
                OrderId = o.Id,
                StockSymbol = o.StockSymbol,
                Quantity = o.Quantity,
                TargetPrice = o.TargetPrice,
                OrderType = o.OrderType,
                Status = o.OrderProcess.Status,
                CreatedDate = o.CreatedDate
            }).ToList();
            var userAlerts = await _context.StockPriceAlerts
                .Where(a => a.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var alertDetails = userAlerts.Select(a => new ShowPortfolioResponse.StockPriceAlertDetail
            {
                AlertId = a.Id,
                StockSymbol = a.StockSymbol,
                TargetPrice = a.TargetPrice,
                AlertType = a.AlertType,
                IsTriggered = a.IsTriggered,
                TriggeredDate = a.TriggeredDate,
                CreatedDate = a.CreatedDate
            }).ToList();

            return Result.Success(new ShowPortfolioResponse
            {
                IsSuccess = true,
                Message = "Portfolio retrieved successfully.",
                StockHoldingItems = stockHoldingItems,
                Orders = orderDetails,
                StockPriceAlerts = alertDetails,
                Transactions = transactionDetails, 
                Change = totalChange,
                TotalPortfolioValue = totalPortfolioValue,
                TotalBalance = totalBalance,
                AvailableBalance = availableBalance
            });
        }
    }
}
