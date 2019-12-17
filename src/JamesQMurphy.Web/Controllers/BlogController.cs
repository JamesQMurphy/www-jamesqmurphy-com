using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models.BlogViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    public class blogController : JqmControllerBase
    {
        private readonly IArticleStore articleStore;
        private readonly IMarkdownHtmlRenderer htmlRenderer;

        public blogController(IArticleStore iarticleStore, IMarkdownHtmlRenderer ihtmlRenderer)
        {
            articleStore = iarticleStore;
            htmlRenderer = ihtmlRenderer;
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
                htmlContent = htmlRenderer.RenderHtml(c.Content),
                replyToId = c.ReplyToId
            }));
        }

        [HttpPost]
        [ActionName("comments")]
        //[Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> commentsPost(string year, string month, string slug, string userComment, string replyTo = "")
        {
            var retVal = await articleStore.AddComment($"{year}/{month}/{slug}", userComment, CurrentUserId, CurrentUserName, DateTime.UtcNow, replyTo);
            if (retVal)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}