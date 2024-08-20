using Core.Data.Entity;
using Core.Data;
using Core.Features.Deposit;
using Core.Shared;
using MediatR;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

internal class DepositHandler : IRequestHandler<DepositCommand, Result<DepositResponse>>
{
    private readonly ApplicationDbContext _context;
    private readonly CurrencyConversionService _currencyConversionService;
    private readonly IValidator<DepositCommand> _validator;

    public DepositHandler(
        ApplicationDbContext context,
        CurrencyConversionService currencyConversionService,
        IValidator<DepositCommand> validator)
    {
        _context = context;
        _currencyConversionService = currencyConversionService;
        _validator = validator;
    }

    public async Task<Result<DepositResponse>> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<DepositResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
        }

        // Convert the amount to USD
        var convertedAmount = await _currencyConversionService.ConvertToUSD(request.Amount, request.Currency);

        // Create and save the transaction
        var transaction = new Transaction
        {
            UserId = request.UserId,
            Amount = convertedAmount,
            Type = TransActionType.Positive,
            Description = "Deposit"
        };

        _context.Transactions.Add(transaction);

        // Update the user's balance
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure<DepositResponse>(new Error("UserNotFound", "User not found."));
        }

        user.Balance += convertedAmount;

        // Save changes to the database
        await _context.SaveChangesAsync(cancellationToken);

        // Return a successful response
        return Result.Success(new DepositResponse
        {
            IsSuccess = true,
            NewBalance = user.Balance,
            USDConvertedAmount = convertedAmount, // Include the converted USD amount
            Message = "Deposit successful."
        });
    }
}
