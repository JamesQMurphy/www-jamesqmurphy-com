using JamesQMurphy.Web.Models.ViewComponentModels;
using Microsoft.AspNetCore.Mvc;

namespace JamesQMurphy.Web.ViewComponents
{
    public class Article : ViewComponent
    {
        public IViewComponentResult Invoke(JamesQMurphy.Blog.Article article)
        {
            return View(article);
        }
    }
}
