using FluentValidation;

namespace Core.Features.Deposit
{
    public class DepositValidator : AbstractValidator<DepositCommand>
    {
        public DepositValidator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("User ID is required.");
            RuleFor(c => c.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
