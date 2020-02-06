using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    [Authorize]
    public class profileController : JqmControllerBase
    {
        public profileController(WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
        }

        [HttpGet]
        public IActionResult index()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Profile";
            return View();
        }
    }
}
