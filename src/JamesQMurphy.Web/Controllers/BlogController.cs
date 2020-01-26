﻿using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.BlogViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class blogController : JqmControllerBase
    {
        private readonly IArticleStore articleStore;
        private readonly IMarkdownHtmlRenderer _markdownHtmlRenderer;

        public blogController(IArticleStore iarticleStore, IMarkdownHtmlRenderer markdownHtmlRenderer, WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
            articleStore = iarticleStore;
            _markdownHtmlRenderer = markdownHtmlRenderer;
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
            ViewData["isLoggedIn"] = this.IsLoggedIn;
            ViewData["returnUrl"] = $"{HttpContext?.Request?.Path}#addComment";
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
                return View(article);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> rss()
        {
            var sb = new StringBuilder("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\"><channel>");
            sb.AppendFormat("<title>{0}</title><link>{1}</link>", WebSiteOptions.WebSiteTitle, ToAbsoluteUrl("/"));
            sb.AppendFormat("<description>{0}</description>", "Cold brew is awesome.  So is DevOps.");
            sb.AppendFormat("<atom:link href=\"{0}\" rel=\"self\" type=\"application/rss+xml\" />", ToAbsoluteUrl($"/blog/{nameof(rss)}"));
            sb.AppendFormat("<image><url>{0}</url><title>{1}</title><link>{2}</link></image>", ToAbsoluteUrl("/apple-touch-icon.png"), WebSiteOptions.WebSiteTitle, ToAbsoluteUrl("/"));
            sb.Append("<language>en-us</language>");

            foreach(var article in await articleStore.GetLastArticlesAsync(WebSiteOptions.ArticlesInRss))
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

        public async Task<IActionResult> comments(string year, string month, string slug, string sinceTimestamp = "")
        {
            var comments = await articleStore.GetArticleComments($"{year}/{month}/{slug}", sinceTimestamp, 1);
            var thisUserId = CurrentUserId;

            return new JsonResult(comments.Select(c => new BlogArticleComment {
                commentId = c.CommentId,
                articleSlug = c.ArticleSlug,
                authorName = c.AuthorName,
                authorImageUrl = "/images/unknownPersonPlaceholder.png",
                timestamp = c.PublishDate.ToString("O"),
                isMine = (c.AuthorId == thisUserId),
                htmlContent = _markdownHtmlRenderer.RenderHtmlSafe(c.Content),
                replyToId = c.ReplyToId
            }));
        }

        [HttpPost]
        [ActionName("comments")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> commentsPost(string year, string month, string slug, string userComment, string sinceTimestamp = "", string replyTo = "")
        {
            var retVal = await articleStore.AddComment($"{year}/{month}/{slug}", userComment, CurrentUserId, CurrentUserName, DateTime.UtcNow, replyTo);
            if (retVal)
            {
                return await this.comments(year, month, slug, sinceTimestamp);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}