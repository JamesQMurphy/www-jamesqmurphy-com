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
        private Article[] articleList = new Article[3] {
                new Article() { Title = "Article 1", Slug = "article-1", Content = "This is article one."},
                new Article() { Title = "Article 2", Slug = "article-2", Content = "This is article two." },
                new Article() { Title = "Article 3", Slug = "article-3", Content = "This is article three." }
            };


        public IActionResult Index()
        {
            var articleMetadataList = articleList.Select(a => a.Metadata);
            return View(articleMetadataList);
        }

        public IActionResult Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction("Index");
            }

            var article = articleList.Where(a => a.Slug == slug).FirstOrDefault();
            if (article != null)
            {
                return View(article);
            }
            else
            {
                return NotFound();
            }
        }
    }
}