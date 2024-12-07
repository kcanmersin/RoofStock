using MediatR;
using Microsoft.AspNetCore.Identity;
using Core.Shared;
using Core.Data.Entity.User;
using Core.Service.JWT;
using System.Security.Claims;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.WebUtilities; 

namespace Core.Features.User.ConfirmEmail
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result<ConfirmEmailResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;

        public ConfirmEmailHandler(UserManager<AppUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<Result<ConfirmEmailResponse>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var principal = _jwtService.ValidateToken(decodedToken);
            if (principal == null)
            {
                return Result.Failure<ConfirmEmailResponse>(new Error("InvalidToken", "Invalid confirmation token."));
            }

            var emailClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (emailClaim == null || emailClaim.Value != request.Email)
            {
                return Result.Failure<ConfirmEmailResponse>(new Error("InvalidToken", "Token does not match the email."));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<ConfirmEmailResponse>(new Error("UserNotFound", "User not found."));
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null; 
            user.EmailConfirmationSentAt = null; 

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure<ConfirmEmailResponse>(new Error("UpdateFailed", "Could not confirm email."));
            }

            return Result.Success(new ConfirmEmailResponse { IsSuccess = true });
        }

    }
}
