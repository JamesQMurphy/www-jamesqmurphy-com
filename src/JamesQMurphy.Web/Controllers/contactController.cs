using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ContactViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class contactController : JqmControllerBase
    {
        private readonly IArticleStore _articleStore;

        public contactController(IArticleStore articleStore, WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
            _articleStore = articleStore;
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
                await _articleStore.AddReaction(Constants.SLUG_CONTACT_US, ArticleReactionType.Comment, model.Comments, CurrentUserId, CurrentUserName, System.DateTime.UtcNow);
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
