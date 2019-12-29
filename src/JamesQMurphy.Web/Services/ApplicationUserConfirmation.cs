using JamesQMurphy.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class ApplicationUserConfirmation : IUserConfirmation<ApplicationUser>
    {
        public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }
    }
}
