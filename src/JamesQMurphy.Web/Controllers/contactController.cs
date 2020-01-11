using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ContactViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class contactController : JqmControllerBase
    {
        private readonly IEmailGenerator _emailGenerator;
        private readonly WebSiteOptions _webSiteOptions;

        public contactController(IEmailGenerator emailGenerator, WebSiteOptions webSiteOptions)
        {
            _emailGenerator = emailGenerator;
            _webSiteOptions = webSiteOptions;
        }

        [HttpGet]
        public IActionResult index()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Get In Touch";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> index(IndexViewModel model)
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Get In Touch";
            if (ModelState.IsValid)
            {
                await _emailGenerator.GenerateEmailAsync(_webSiteOptions.CommentsEmail, EmailType.Comments, new string[] { CurrentUserId, model.Comments});
                return RedirectToAction(nameof(commentsConfirmation));
            }
            return View();
        }

        [HttpGet]
        public IActionResult commentsConfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Thanks For Getting In Touch";
            return View();
        }
    }
}
