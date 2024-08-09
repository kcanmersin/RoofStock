using FluentValidation;

namespace Core.Features.Withdrawal
{
    public class WithdrawalValidator : AbstractValidator<WithdrawalCommand>
    {
        public WithdrawalValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.AmountInUSD)
                .GreaterThan(0).WithMessage("The withdrawal amount must be greater than zero.");

            RuleFor(x => x.TargetCurrency)
                .NotEmpty().WithMessage("Target currency is required.")
                .Must(BeAValidCurrency).WithMessage("Invalid target currency.");
        }

        private bool BeAValidCurrency(string currency)
        {
            var validCurrencies = new[] { "USD", "EUR", "GBP", "TRY", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD" };

            return validCurrencies.Contains(currency.ToUpper());
        }
    }
}
