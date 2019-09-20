using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JamesQMurphy.Web.Controllers
{
    public abstract class JqmControllerBase : Controller
    {
        private ApplicationUser _applicationUser = null;

        protected string CurrentUserName => this.User?.Identity?.Name;
        protected bool IsLoggedIn => (this.User?.Identity?.IsAuthenticated == true);

        protected async Task<ApplicationUser> GetApplicationUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (_applicationUser == null && CurrentUserName != null)
            {
                _applicationUser = await userManager.GetUserAsync(this.User);
            }
            return _applicationUser;
        }
    }
}
