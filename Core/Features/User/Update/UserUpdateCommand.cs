using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Shared;
using MediatR;

namespace Core.Features.User.Update
{
    public class UserUpdateCommand : IRequest<Result<UserUpdateResponse>>
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}