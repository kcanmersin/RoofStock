using MediatR;
using Core.Shared;

namespace Core.Features.User.Register
{
    
    public class RegisterCommand : IRequest<Result<RegisterResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
