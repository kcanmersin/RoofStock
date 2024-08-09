using MediatR;
using Microsoft.AspNetCore.Identity;
using Core.Shared;
using System.Linq;
using Core.Data.Entity.User;
using Core.Service.JWT;

namespace Core.Features.User.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtService _jwtService;

        public RegisterHandler(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<RegisterResponse>(new Error("RegistrationFailed", string.Join(", ", errors)));
            }

            if (!string.IsNullOrEmpty(request.RoleName))
            {
                if (!await _roleManager.RoleExistsAsync(request.RoleName))
                {
                    await _roleManager.CreateAsync(new AppRole { Name = request.RoleName });
                }
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var token = _jwtService.GenerateToken(user.Email, user.Id);

            var response = new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = token,
                IsSuccess = true
            };

            return Result.Success(response);
        }
    }
}
