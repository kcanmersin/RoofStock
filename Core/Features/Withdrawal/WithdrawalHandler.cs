using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Core.Data;
using Core.Data.Entity.User;
using Core.Shared;
using Core.Data.Entity;

namespace Core.Features.Withdrawal
{
    public class WithdrawalHandler : IRequestHandler<WithdrawalCommand, Result<WithdrawalResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyConversionService _currencyConversionService;
        private readonly IValidator<WithdrawalCommand> _validator;

        public WithdrawalHandler(
            ApplicationDbContext context,
            CurrencyConversionService currencyConversionService,
            IValidator<WithdrawalCommand> validator)
        {
            _context = context;
            _currencyConversionService = currencyConversionService;
            _validator = validator;
        }

        public async Task<Result<WithdrawalResponse>> Handle(WithdrawalCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<WithdrawalResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<WithdrawalResponse>(new Error("UserNotFound", "User not found."));
            }

            if (user.Balance < request.AmountInUSD)
            {
                return Result.Failure<WithdrawalResponse>(new Error("InsufficientFunds", "User does not have sufficient balance."));
            }

            decimal convertedAmount;
            if (request.TargetCurrency.ToUpper() == "USD")
            {
                convertedAmount = request.AmountInUSD;
            }
            else
            {
                convertedAmount = await _currencyConversionService.ConvertFromUSD(request.AmountInUSD, request.TargetCurrency);
            }

            user.Balance -= request.AmountInUSD;

            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = -request.AmountInUSD, 
                Type = TransActionType.Negative,
                Description = $"Withdrawal of {convertedAmount} {request.TargetCurrency} (equivalent to {request.AmountInUSD} USD)"
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new WithdrawalResponse
            {
                IsSuccess = true,
                NewBalance = user.Balance,
                ConvertedAmount = convertedAmount,
                TargetCurrency = request.TargetCurrency,
                Message = "Withdrawal successful."
            });
        }
    }
}