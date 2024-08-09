using MediatR;
using Core.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Core.Data.Entity;
using Core.Data;
using Core.Service.StockApi;

namespace Core.Features.SellStock
{
    public class SellStockHandler : IRequestHandler<SellStockCommand, Result<SellStockResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IValidator<SellStockCommand> _validator;

        public SellStockHandler(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            IValidator<SellStockCommand> validator)
        {
            _context = context;
            _stockApiService = stockApiService;
            _validator = validator;
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

            var stockHolding = await _context.StockHoldings
                .FirstOrDefaultAsync(sh => sh.UserId == request.UserId && sh.StockSymbol == request.StockSymbol);

            if (stockHolding == null || stockHolding.Quantity < request.Quantity)
            {
                return Result.Failure<SellStockResponse>(new Error("InsufficientHoldings", "User does not have enough stock holdings to sell."));
            }

            var totalProceeds = currentPrice * request.Quantity;

            stockHolding.Quantity -= request.Quantity;
            stockHolding.TotalPurchasePrice -= totalProceeds;

            if (stockHolding.Quantity == 0)
            {
                _context.StockHoldings.Remove(stockHolding);
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<SellStockResponse>(new Error("UserNotFound", "User not found."));
            }

            user.Balance += totalProceeds;

            var stockHoldingItem = new StockHoldingItem
            {
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                UnitPrice = currentPrice,
                Type = StockHoldingItemType.Sale,
                //OrderProcessId = Guid.NewGuid() 
            };
            _context.StockHoldingItems.Add(stockHoldingItem);

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = totalProceeds,
                Type = TransActionType.Positive,
                Description = $"Sale of {request.Quantity} shares of {request.StockSymbol} at {currentPrice} per share",
                StockHoldingItem = stockHoldingItem 
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new SellStockResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance,
                Message = $"Successfully sold {request.Quantity} shares of {request.StockSymbol} at {currentPrice} per share.",
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                TotalProceeds = totalProceeds
            });
        }
    }
}

