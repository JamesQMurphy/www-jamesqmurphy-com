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
        private readonly WebSiteOptions webSiteOptions;
        private readonly UserManager<ApplicationUser> userManager;

        public homeController(IArticleStore iarticleStore, WebSiteOptions webSiteOptions, UserManager<ApplicationUser> userMgr)
        {
            articleStore = iarticleStore;
            this.webSiteOptions = webSiteOptions;
            userManager = userMgr;
        }

        public async Task<IActionResult> index()
        {
            // Need to optimize this
            var lastTwoArticles = await articleStore.GetLastArticlesAsync(2);
            var lastTwoArticlesList = lastTwoArticles.ToList();
            var article1 = await articleStore.GetArticleAsync(lastTwoArticlesList[0].Slug);
            var article2 = await articleStore.GetArticleAsync(lastTwoArticlesList[1].Slug);
            var homePageItems = new HomePageItems(article1, article2);

            return View(homePageItems);
        }

        public IActionResult about()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "About This Site";
            
            return View("_Article", new Article {
                Content = System.IO.File.ReadAllText("Views/Home/About.md")
            });
        }

        public IActionResult privacy()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Privacy Policy";
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;

            return View("_Article", new Article
            {
                Content = System.IO.File.ReadAllText("Views/Home/Privacy.md").Replace("@webSiteTitle", webSiteOptions.WebSiteTitle)
            });
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
