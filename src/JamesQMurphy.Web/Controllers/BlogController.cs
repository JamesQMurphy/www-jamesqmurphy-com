using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class blogController : JqmControllerBase
    {
        private readonly IArticleStore articleStore;
        private readonly IMarkdownHtmlRenderer _markdownHtmlRenderer;
        private readonly WebSiteOptions _webSiteOptions;

        public blogController(IArticleStore iarticleStore, IMarkdownHtmlRenderer markdownHtmlRenderer, WebSiteOptions webSiteOptions)
        {
            articleStore = iarticleStore;
            _markdownHtmlRenderer = markdownHtmlRenderer;
            _webSiteOptions = webSiteOptions;
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
            return View(await articleStore.GetArticleMetadatasAsync(startDate, endDate));
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

        [HttpGet]
        public async Task<IActionResult> rss()
        {
            var sb = new StringBuilder("<rss version=\"2.0\"><channel>");
            sb.AppendFormat("<title>{0}</title><link>{1}</link>", _webSiteOptions.WebSiteTitle, ToAbsoluteUrl("/"));
            sb.AppendFormat("<description>{0}</description>", "Cold brew is awesome.  So is DevOps.");
            sb.AppendFormat("<image>{0}</image>", ToAbsoluteUrl("/apple-touch-icon.png"));
            sb.Append("<language>en-us</language>");

            foreach(var article in await articleStore.GetLastArticlesAsync(_webSiteOptions.ArticlesInRss))
            {
                sb.Append("<item>");
                sb.AppendFormat("<title>{0}", article.Title);
                if (!(String.IsNullOrWhiteSpace(article.Description)))
                {
                    sb.AppendFormat(": {0}", article.Description);
                }
                sb.Append("</title>");
                sb.AppendFormat("<link>{0}</link>", ToAbsoluteUrl($"/blog/{article.Slug}"));
                sb.AppendFormat("<guid isPermaLink=\"false\">{0}</guid>", article.Slug);
                sb.AppendFormat("<pubDate>{0}</pubDate>", article.PublishDate.ToString("r"));
                sb.AppendFormat("<description><![CDATA[{0}]]></description>", _markdownHtmlRenderer.RenderHtml(article.Content));
                sb.Append("</item>");
            }
            

            sb.AppendFormat("</channel></rss>");

            return Content(sb.ToString(), "application/rss+xml");
        }

        private string ToAbsoluteUrl(string relativeUrl)
        {
            return new Uri(new Uri(Request.Scheme + "://" + Request.Host.Value), Url.Content(relativeUrl)).ToString();
        }
    }
}