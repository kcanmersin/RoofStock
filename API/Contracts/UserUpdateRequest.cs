using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.User.Update
{
    public class UserUpdateRequest
    {
        [DefaultValue("657c6705-0728-4dfc-8019-ea59c42f984f")]
        public Guid? UserId { get; set; } 
        [DefaultValue("kerimcanmersin@gmail.com")]
        public string? Email { get; set; } 
        [DefaultValue("Can")]
        public string? FirstName { get; set; } 
        [DefaultValue("Mersin")]
        public string? LastName { get; set; }
        [DefaultValue("+1234567890")]
        public string? PhoneNumber { get; set; }
        [DefaultValue("/API/defaultlogo.jpg")]
        public string? ImagePath { get; set; } 
    }
}
