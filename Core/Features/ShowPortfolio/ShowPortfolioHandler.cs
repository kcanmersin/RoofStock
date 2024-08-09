using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Features.ShowPortfolio;
using Core.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioHandler : IRequestHandler<ShowPortfolioCommand, Result<ShowPortfolioResponse>>
    {
        private readonly ApplicationDbContext _context;

        public ShowPortfolioHandler(ApplicationDbContext context)
        {
            _context = context;
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

            var stockHoldingItems = user.StockHoldings.Select(sh => new ShowPortfolioResponse.StockHoldingItemDetail
            {
                StockSymbol = sh.StockSymbol,
                Quantity = sh.Quantity,
                UnitPrice = sh.TotalPurchasePrice / sh.Quantity, 
            }).ToList();

            return Result.Success(new ShowPortfolioResponse
            {
                IsSuccess = true,
                Message = "Portfolio retrieved successfully.",
                StockHoldingItems = stockHoldingItems
            });
        }
    }
}
