using MediatR;
using Microsoft.AspNetCore.Identity;
using Core.Shared;
using Core.Data.Entity.User;
using Core.Service.JWT;
using FluentValidation;
using Core.Service.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Core.Features.User.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IValidator<RegisterCommand> _validator;
        private readonly IConfiguration _configuration;

        public RegisterHandler(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IEmailService emailService,
            IJwtService jwtService,
            IValidator<RegisterCommand> validator,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _jwtService = jwtService;
            _validator = validator;
            _configuration = configuration;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<RegisterResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsEmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<RegisterResponse>(new Error("RegistrationFailed", string.Join(", ", errors)));
            }

            var emailConfirmationToken = _jwtService.GenerateToken(user.Email, user.Id);
            user.EmailConfirmationToken = emailConfirmationToken;
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(request.RoleName))
            {
                if (!await _roleManager.RoleExistsAsync(request.RoleName))
                {
                    await _roleManager.CreateAsync(new AppRole { Name = request.RoleName });
                }
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }

            var base64EncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            var confirmationUrlBase = Environment.GetEnvironmentVariable("APP_CONFIRMATIONURLBASE")
                                      ?? _configuration["AppSettings:ConfirmationUrlBase"];
            var confirmationLink = $"{confirmationUrlBase}?email={user.Email}&token={base64EncodedToken}";

            var emailSubject = "Please confirm your email address";
            var emailMessage = $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, emailSubject, emailMessage);
            }
            catch (Exception)
            {
                return Result.Failure<RegisterResponse>(new Error("EmailFailed", "Failed to send confirmation email."));
            }

            return Result.Success(new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsSuccess = true,
                Message = "Registration successful! Please check your email to confirm your account."
            });
        }
    }
}
