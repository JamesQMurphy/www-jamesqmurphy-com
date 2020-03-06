using JamesQMurphy.Auth;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    [Authorize]
    public class profileController : JqmControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public profileController(WebSiteOptions webSiteOptions, UserManager<ApplicationUser> userManager) : base(webSiteOptions)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> index(string username = "")
        {
            if (username == "")
            {
                username = CurrentUserName;
            }
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // This gets the proper casing of the username
            username = await _userManager.GetUserNameAsync(user);

            ViewData[Constants.VIEWDATA_PAGETITLE] = $"Profile for {username}";
            ViewData["username"] = username;
            ViewData["isMyAccount"] = (user.UserId == CurrentUserId);
            if (IsLoggedIn)
            {
                var userRecords = (await GetApplicationUserAsync(_userManager)).ApplicationUserRecords;
                ViewData["idRecord"] = userRecords.Where(r => r.Provider == ApplicationUserRecord.RECORD_TYPE_ID).FirstOrDefault();
                ViewData["otherRecords"] = userRecords.Where(r => r.Provider != ApplicationUserRecord.RECORD_TYPE_ID).ToList();
            }
            return View();
        }
    }
}
