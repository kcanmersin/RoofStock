using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.User.Login
{

    public class LoginResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }
}
