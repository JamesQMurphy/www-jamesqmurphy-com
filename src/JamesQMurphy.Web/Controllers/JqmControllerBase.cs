using JamesQMurphy.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

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

        protected IActionResult RedirectToLocal(string returnUrl = null)
        {
            if ((!String.IsNullOrEmpty(returnUrl)) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index).ToLowerInvariant(), "home");
            }
        }

    }
}
