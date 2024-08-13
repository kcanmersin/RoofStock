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
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<RegisterResponse>(new Error("ValidationFailed", validationResult.Errors.First().ErrorMessage));
            }

            // Create the user
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsEmailConfirmed = false
            };

            // Kullanıcıyı kaydet
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<RegisterResponse>(new Error("RegistrationFailed", string.Join(", ", errors)));
            }

            // Kullanıcı kaydedildikten sonra, token oluşturup veritabanına kaydedelim
            var emailConfirmationToken = _jwtService.GenerateToken(user.Email, user.Id);
            user.EmailConfirmationToken = emailConfirmationToken;
            user.EmailConfirmationSentAt = DateTime.UtcNow;

            // Kullanıcıyı güncelleyelim
            await _userManager.UpdateAsync(user);

            // Assign role if specified
            if (!string.IsNullOrEmpty(request.RoleName))
            {
                if (!await _roleManager.RoleExistsAsync(request.RoleName))
                {
                    await _roleManager.CreateAsync(new AppRole { Name = request.RoleName });
                }
                await _userManager.AddToRoleAsync(user, request.RoleName);
            }

            // Token'ı Base64 formatında encode et
            var base64EncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            // Retrieve the confirmation URL base from appsettings.json
            var confirmationUrlBase = _configuration["AppSettings:ConfirmationUrlBase"];
            var confirmationLink = $"{confirmationUrlBase}?email={user.Email}&token={base64EncodedToken}";

            // Send confirmation email
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

            // Kayıt başarılı olduğunda döndürülen yanıt
            return Result.Success(new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsSuccess = true
            });
        }
    }
}
