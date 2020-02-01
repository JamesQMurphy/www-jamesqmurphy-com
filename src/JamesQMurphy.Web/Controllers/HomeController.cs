using JamesQMurphy.Auth;
using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class homeController : JqmControllerBase
    {
        private readonly IArticleStore articleStore;
        private readonly UserManager<ApplicationUser> userManager;

        public homeController(IArticleStore iarticleStore, WebSiteOptions webSiteOptions, UserManager<ApplicationUser> userMgr) : base(webSiteOptions)
        {
            articleStore = iarticleStore;
            userManager = userMgr;
        }

        public async Task<IActionResult> index()
        {
            // Need to optimize this
            var lastTwoArticles = await articleStore.GetLastArticlesAsync(2);
            var lastTwoArticlesList = lastTwoArticles.ToList();
            var homePageItems = new HomePageItems(lastTwoArticlesList[0], lastTwoArticlesList[1]);

            return View(homePageItems);
        }

        public IActionResult about()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "About This Site";
            var article = new Article
            {
                Content = System.IO.File.ReadAllText("Views/Home/About.md")
            };
            return View("Details", article);
        }

        public IActionResult privacy()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Privacy Policy";
            var article = new Article
            {
                Content = System.IO.File.ReadAllText("Views/Home/Privacy.md").Replace("@webSiteTitle", WebSiteOptions.WebSiteTitle)
            };
            return View("Details", article);
        }

        public IActionResult terms()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Terms and Conditions";
            var article = new Article
            {
                Content = System.IO.File.ReadAllText("Views/Home/TermsAndConditions.md").Replace("@webSiteTitle", WebSiteOptions.WebSiteTitle)
            };
            return View("Details", article);
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> secret()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Secret Page";
            ViewData["User"] = await GetApplicationUserAsync(userManager);
            return View("Secret");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
