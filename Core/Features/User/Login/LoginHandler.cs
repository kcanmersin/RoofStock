using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity.User;
using Core.Features.User.Register;
using Core.Service.JWT;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Core.Features.User.Login
{
   public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtService _jwtService;

        public LoginHandler(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<LoginResponse>(new Error("LoginFailed", "Invalid email or password."));
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