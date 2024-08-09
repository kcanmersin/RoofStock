using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.User.Update
{
    public class UserUpdateRequest
    {
        [DefaultValue("45548f08-b7ce-4dc9-8901-a379d31634aa")]
        public Guid UserId { get; set; } 
        [DefaultValue("can_updated@gmail.com")]
        public string Email { get; set; } 
        [DefaultValue("Can")]
        public string FirstName { get; set; } 
        [DefaultValue("Mersin")]
        public string LastName { get; set; }
        [DefaultValue("+1234567890")]
        public string PhoneNumber { get; set; }
        [DefaultValue("/API/defaultlogo.jpg")]
        public string ImagePath { get; set; } 
    }
}
