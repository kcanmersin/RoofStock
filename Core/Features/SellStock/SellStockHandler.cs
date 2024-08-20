using MediatR;
using Core.Shared;
using FluentValidation;
using Core.Data;
using Core.Data.Entity;
using Core.Service.StockApi;
using Microsoft.EntityFrameworkCore;

namespace Core.Features.SellStock
{
    internal class SellStockHandler : IRequestHandler<SellStockCommand, Result<SellStockResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IValidator<SellStockCommand> _validator;
        private readonly ISellService _sellService;

        public SellStockHandler(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            IValidator<SellStockCommand> validator,
            ISellService sellService)
        {
            _context = context;
            _stockApiService = stockApiService;
            _validator = validator;
            _sellService = sellService;
        }

        public async Task<Result<SellStockResponse>> Handle(SellStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SellStockResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var currentPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);
            if (currentPrice <= 0)
            {
                return Result.Failure<SellStockResponse>(new Error("InvalidStockPrice", "Could not retrieve a valid stock price."));
            }

            var user = await _context.Users.Include(u => u.StockHoldings)
                                           .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<SellStockResponse>(new Error("UserNotFound", "User not found."));
            }

            var totalPendingQuantity = await _context.Orders
                .Where(o => o.UserId == request.UserId
                            && o.OrderType == OrderType.Sell
                            && o.OrderProcess.Status == OrderProcessStatus.Pending
                            && o.StockSymbol == request.StockSymbol)
                .SumAsync(o => o.Quantity);

            var stockHolding = user.StockHoldings.FirstOrDefault(sh => sh.StockSymbol == request.StockSymbol);

            if (stockHolding == null || stockHolding.Quantity < totalPendingQuantity + request.Quantity)
            {
                return Result.Failure<SellStockResponse>(new Error("InsufficientHoldings", "User does not have sufficient available stock holdings."));
            }

            var result = await _sellService.SellStockAsync(user, request.StockSymbol, request.Quantity, currentPrice);

            if (!result.IsSuccess)
            {
                return Result.Failure<SellStockResponse>(result.Error);
            }

            return Result.Success(new SellStockResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance ,
                Message = $"Successfully sold {request.Quantity} shares of {request.StockSymbol} at {currentPrice:C} per share.",
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                TotalProceeds = currentPrice * request.Quantity
            });
        }
    }
}
