using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity.User;
using Microsoft.AspNetCore.Identity;
using Quartz;

namespace Core.Service.DeleteUnconfirmedUsers
{

    public class DeleteUnconfirmedUsersJob : IJob
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteUnconfirmedUsersJob(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var confirmationTimeLimit = TimeSpan.FromMinutes(60);

            var usersToDelete = _userManager.Users
                .Where(u => !u.IsEmailConfirmed && u.EmailConfirmationSentAt.HasValue
                            && DateTime.UtcNow - u.EmailConfirmationSentAt > confirmationTimeLimit)
                .ToList();

            foreach (var user in usersToDelete)
            {
                await _userManager.DeleteAsync(user);
            }
        }
    }
}