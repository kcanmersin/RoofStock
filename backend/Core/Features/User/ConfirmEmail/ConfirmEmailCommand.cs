using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Shared;
using MediatR;

namespace Core.Features.User.ConfirmEmail
{
public class ConfirmEmailCommand : IRequest<Result<ConfirmEmailResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}