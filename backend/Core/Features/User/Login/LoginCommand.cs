using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Features.User.Register;
using Core.Shared;
using MediatR;

namespace Core.Features.User.Login
{
    public class LoginCommand : IRequest<Result<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}