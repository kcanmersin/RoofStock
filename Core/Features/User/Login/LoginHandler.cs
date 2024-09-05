using Core.Data.Entity.User;
using Core.Features.User.Register;
using Core.Service.JWT;
using Core.Shared;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Core.Features.User.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IValidator<LoginCommand> _validator;

        public LoginHandler(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtService jwtService, IValidator<LoginCommand> validator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _validator = validator;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<LoginResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<LoginResponse>(new Error("LoginFailed", "Invalid email or password."));
            }

            // Check if the user's email is confirmed
            if (!user.IsEmailConfirmed)
            {
                return Result.Failure<LoginResponse>(new Error("LoginFailed", "Email not confirmed. Please confirm your email to log in."));
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Result.Failure<LoginResponse>(new Error("LoginFailed", "Invalid email or password."));
            }

            var token = _jwtService.GenerateToken(user.Email, user.Id);

            var response = new LoginResponse
            {
                UserId = user.Id.ToString(),
                Token = token
            };

            return Result.Success(response);
        }
    }
}
