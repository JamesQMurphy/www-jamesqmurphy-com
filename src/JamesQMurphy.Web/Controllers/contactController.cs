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

        public contactController(IEmailGenerator emailGenerator, WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
            _emailGenerator = emailGenerator;
        }

        [HttpGet]
        public IActionResult index()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Get In Touch";
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> index(IndexViewModel model)
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Get In Touch";
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            if (ModelState.IsValid)
            {
                await _emailGenerator.GenerateEmailAsync(WebSiteOptions.CommentsEmail, EmailType.Comments, new string[] { CurrentUserName, model.Comments});
                return RedirectToAction(nameof(commentsConfirmation));
            }
            return View();
        }

        [HttpGet]
        public IActionResult commentsConfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Thanks For Getting In Touch";
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            return View();
        }
    }
}
