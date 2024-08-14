using MediatR;
using Core.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Core.Data.Entity;
using Core.Data;
using Core.Service.StockApi;

namespace Core.Features.BuyStock
{
    public class BuyStockHandler : IRequestHandler<BuyStockCommand, Result<BuyStockResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IValidator<BuyStockCommand> _validator;

        public BuyStockHandler(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            IValidator<BuyStockCommand> validator)
        {
            _context = context;
            _stockApiService = stockApiService;
            _validator = validator;
        }

        public async Task<Result<BuyStockResponse>> Handle(BuyStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<BuyStockResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }
            //_buystockservice.buy()
            var currentPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);
            if (currentPrice <= 0)
            {
                return Result.Failure<BuyStockResponse>(new Error("InvalidStockPrice", "Could not retrieve a valid stock price."));
            }

            var totalCost = currentPrice * request.Quantity;

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<BuyStockResponse>(new Error("UserNotFound", "User not found."));
            }

            if (user.Balance < totalCost)
            {
                return Result.Failure<BuyStockResponse>(new Error("InsufficientFunds", "User does not have sufficient balance."));
            }

            user.Balance -= totalCost;

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == request.UserId && sh.StockSymbol == request.StockSymbol);

            if (stockHolding == null)
            {
                stockHolding = new StockHolding
                {
                    UserId = request.UserId,
                    StockSymbol = request.StockSymbol,
                    Quantity = request.Quantity,
                    TotalPurchasePrice = totalCost
                };
                _context.StockHoldings.Add(stockHolding);
            }
            else
            {
                stockHolding.Quantity += request.Quantity;
                stockHolding.TotalPurchasePrice += totalCost;
            }

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = -totalCost, 
                Type = TransActionType.Negative,
                Description = $"Purchase of {request.Quantity} shares of {request.StockSymbol} at {currentPrice} per share"
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new BuyStockResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance,
                Message = $"Successfully purchased {request.Quantity} shares of {request.StockSymbol} at {currentPrice} per share.",
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity
            });
        }
    }
}
