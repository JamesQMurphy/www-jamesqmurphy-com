using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JamesQMurphy.Blog;

namespace JamesQMurphy.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IArticleStore articleStore;

        public BlogController(IArticleStore iarticleStore)
        {
            articleStore = iarticleStore;
        }

        public IActionResult Index()
        {
            return View(articleStore.GetArticles());
        }

        public IActionResult Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction("Index");
            }

            var articleMetadata = articleStore.GetArticles().Where(a => a.Slug == slug).FirstOrDefault();
            if (articleMetadata != null)
            {
                var article = articleStore.GetArticle(articleMetadata.YearString, articleMetadata.MonthString, slug);
                return View(article);
            }
            else
            {
                return NotFound();
            }
        }
    }
}