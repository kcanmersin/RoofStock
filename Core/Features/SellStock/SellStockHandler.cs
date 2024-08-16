using MediatR;
using Core.Shared;
using FluentValidation;
using Core.Data.Entity;
using Core.Data;
using Core.Service.StockApi;
using Core.Features;

namespace Core.Features.SellStock
{
    public class SellStockHandler : IRequestHandler<SellStockCommand, Result<SellStockResponse>>
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
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<SellStockResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            // Get the current stock price
            var currentPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);
            if (currentPrice <= 0)
            {
                return Result.Failure<SellStockResponse>(new Error("InvalidStockPrice", "Could not retrieve a valid stock price."));
            }

            // Find the user
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<SellStockResponse>(new Error("UserNotFound", "User not found."));
            }

            // Use SellService to perform the selling operation
            var result = await _sellService.SellStockAsync(user, request.StockSymbol, request.Quantity, currentPrice);

            if (!result.IsSuccess)
            {
                return Result.Failure<SellStockResponse>(result.Error);
            }

            return Result.Success(new SellStockResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance,
                Message = $"Successfully sold {request.Quantity} shares of {request.StockSymbol} at {currentPrice} per share.",
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                TotalProceeds = currentPrice * request.Quantity
            });
        }
    }
}