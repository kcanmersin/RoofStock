using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Core.Features.User.Update
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateCommand>
    {
        public UserUpdateValidator()
        {
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.Email).NotEmpty().EmailAddress();
            RuleFor(c => c.FirstName).NotEmpty().MinimumLength(2);
            RuleFor(c => c.LastName).NotEmpty().MinimumLength(2);
            RuleFor(c => c.PhoneNumber).NotEmpty().MinimumLength(10);
        }
    }
}
