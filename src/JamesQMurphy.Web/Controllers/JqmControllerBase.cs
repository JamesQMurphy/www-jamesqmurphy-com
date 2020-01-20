using JamesQMurphy.Auth;
using JamesQMurphy.Web.Extensions;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public abstract class JqmControllerBase : Controller
    {
        public WebSiteOptions WebSiteOptions { get; private set; }

        private AlertMessageCollection _alertMessageCollection = null;
        public AlertMessageCollection AlertMessageCollection
        {
            get
            {
                if (_alertMessageCollection == null && TempData != null)
                {
                    _alertMessageCollection = new AlertMessageCollection(TempData);
                }
                return _alertMessageCollection;
            }
        }

        private ApplicationUser _applicationUser = null;

        protected string CurrentUserName => this.User?.Identity?.Name;
        protected string CurrentUserId => this.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        protected bool IsLoggedIn => (this.User?.Identity?.IsAuthenticated == true);

        public JqmControllerBase(WebSiteOptions webSiteOptions)
        {
            WebSiteOptions = webSiteOptions;
        }

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
                return RedirectToAction(nameof(homeController.index).ToLowerInvariant(), "home");
            }
        }

        protected string ToAbsoluteUrl(string url)
        {
            if (Url.IsLocalUrl(url))
            {
                return new Uri(new Uri(WebSiteOptions.GetSiteUrlFallbackToContext(HttpContext)), Url.Content(url)).ToString();
            }
            else
            {
                return url;
            }
        }

    }
}
