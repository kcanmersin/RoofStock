using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data.Entity.User;
using Core.Shared;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Core.Features.User.Update
{
    public class UserUpdateHandler : IRequestHandler<UserUpdateCommand, Result<UserUpdateResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<UserUpdateCommand> _validator;

        public UserUpdateHandler(UserManager<AppUser> userManager, IValidator<UserUpdateCommand> validator)
        {
            _userManager = userManager;
            _validator = validator;
        }

        public async Task<Result<UserUpdateResponse>> Handle(UserUpdateCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<UserUpdateResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result.Failure<UserUpdateResponse>(new Error("UserNotFound", "User not found."));
            }

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.ImagePath = request.ImagePath;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<UserUpdateResponse>(new Error("UpdateFailed", string.Join(", ", errors)));
            }

            var response = new UserUpdateResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImagePath = user.ImagePath
            };

            return Result.Success(response);
        }
    }
}