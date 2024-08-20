using MediatR;
using Core.Shared;
using FluentValidation;
using Core.Data;
using Core.Features;
using Core.Service.StockApi;
using Microsoft.EntityFrameworkCore;
using Core.Data.Entity;

namespace Core.Features.BuyStock
{
    public class BuyStockHandler : IRequestHandler<BuyStockCommand, Result<BuyStockResponse>>
    {
        private readonly IBuyService _buyService;
        private readonly IStockApiService _stockApiService;
        private readonly IValidator<BuyStockCommand> _validator;
        private readonly ApplicationDbContext _context;

        public BuyStockHandler(
            IBuyService buyService,
            IStockApiService stockApiService,
            IValidator<BuyStockCommand> validator,
            ApplicationDbContext context)
        {
            _buyService = buyService;
            _stockApiService = stockApiService;
            _validator = validator;
            _context = context;
        }

        public async Task<Result<BuyStockResponse>> Handle(BuyStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<BuyStockResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var currentPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);
            if (currentPrice <= 0)
            {
                return Result.Failure<BuyStockResponse>(new Error("InvalidStockPrice", "Could not retrieve a valid stock price."));
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<BuyStockResponse>(new Error("UserNotFound", "User not found."));
            }

            // Kullanýcýnýn tüm açýk buy emirlerini hesaba katarak mevcut kullanýlabilir bakiyeyi hesaplayýn
            var totalPendingAmount = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.OrderType == OrderType.Buy && o.OrderProcess.Status == OrderProcessStatus.Pending)
                .SumAsync(o => o.Quantity * o.TargetPrice);

            var availableBalance = user.Balance - totalPendingAmount;
            var totalCost = currentPrice * request.Quantity;

            if (availableBalance < totalCost)
            {
                return Result.Failure<BuyStockResponse>(new Error("InsufficientFunds", "User does not have sufficient available balance."));
            }

            var result = await _buyService.BuyStockAsync(user, request.StockSymbol, request.Quantity, currentPrice);

            if (!result.IsSuccess)
            {
                return Result.Failure<BuyStockResponse>(result.Error);
            }

            return Result.Success(new BuyStockResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance - totalCost,
                Message = $"Successfully purchased {request.Quantity} shares of {request.StockSymbol} at {currentPrice:C} per share.",
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity
            });
        }
    }
}
