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

        public IActionResult Index(string year = null, string month = null)
        {
            return View(articleStore.GetArticles(year, month));
        }

        public IActionResult Details(string year, string month, string slug)
        {
            var article = articleStore.GetArticle(year, month, slug);
            if (article != null)
            {
                ViewData["Title"] = article.Title;
                return View(article);
            }
            else
            {
                return NotFound();
            }
        }
    }
}