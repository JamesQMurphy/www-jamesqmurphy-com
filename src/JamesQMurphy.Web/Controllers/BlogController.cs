using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JamesQMurphy.Blog;

namespace JamesQMurphy.Web.Controllers
{
    public class blogController : JqmControllerBase
    {
        private readonly IArticleStore articleStore;

        public blogController(IArticleStore iarticleStore)
        {
            articleStore = iarticleStore;
        }

        public async Task<IActionResult> index(string year = null, string month = null)
        {
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            if (year != null)
            {
                if (month != null)
                {
                    startDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
                    endDate = startDate.AddMonths(1).AddMilliseconds(-1);
                }
                else
                {
                    startDate = new DateTime(Convert.ToInt32(year), 1, 1);
                    endDate = startDate.AddYears(1).AddMilliseconds(-1);
                }
            }
            return View(await articleStore.GetArticlesAsync(startDate, endDate));
        }

        public async Task<IActionResult> details(string year, string month, string slug)
        {
            var article = await articleStore.GetArticleAsync($"{year}/{month}/{slug}");
            if (article != null)
            {
                if (String.IsNullOrWhiteSpace(article.Description))
                {
                    ViewData[Constants.VIEWDATA_PAGETITLE] = article.Title;
                }
                else
                {
                    ViewData[Constants.VIEWDATA_PAGETITLE] = $"{article.Title}: {article.Description}";
                }
                ViewData[Constants.VIEWDATA_MARKDOWN] = article.Content;
                return View("Article", article.Metadata);
            }
            else
            {
                return NotFound();
            }
        }
    }
}