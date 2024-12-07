using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioValidator : AbstractValidator<ShowPortfolioCommand>
    {
        public ShowPortfolioValidator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
