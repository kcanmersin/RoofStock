using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.User.Login
{
    public class LoginRequest
    {
        [DefaultValue("can@gmail.com")]
        public string Email { get; set; } 
        [DefaultValue("19071907")]
        public string Password { get; set; } 
    }
}