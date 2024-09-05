using System.ComponentModel;

namespace Core.Features.User.Register
{
    public class RegisterRequest
    {
        [DefaultValue("kcanmersin@gmail.com")]
        public string? Email { get; set; } 
        [DefaultValue("19071907")]
        public string? Password { get; set; } 
        [DefaultValue("Can")]
        public string? FirstName { get; set; } 
        [DefaultValue("Mersin")]
        public string? LastName { get; set; }
        [DefaultValue("+1234567890")]
        public string? PhoneNumber { get; set; }
        [DefaultValue("User")]
        public string? RoleName { get; set; } 
        

    }
}
