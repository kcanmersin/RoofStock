using FluentValidation;

namespace Core.Features.GiveOrder
{
    public class OrderValidator : AbstractValidator<OrderCommand>
    {
        public OrderValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.StockSymbol)
                .NotEmpty().WithMessage("Stock symbol is required.")
                .Length(1, 5).WithMessage("Stock symbol length must be between 1 and 5 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.TargetPrice)
                .GreaterThan(0).WithMessage("Target price must be greater than zero.");

            RuleFor(x => x.OrderType)
                .IsInEnum().WithMessage("Invalid order type.");
        }
    }
}
