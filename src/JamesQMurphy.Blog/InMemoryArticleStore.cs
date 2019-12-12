using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class InMemoryArticleStore : IArticleStore
    {
        public readonly List<Article> Articles = new List<Article>();
        private readonly Dictionary<string, SortedSet<ArticleComment>> _articleComments = new Dictionary<string, SortedSet<ArticleComment>>();

        public Task<Article> GetArticleAsync(string slug)
        {
            return Task.FromResult(Articles.FindLast(a => a.Slug == slug));
        }

        public Task<IEnumerable<ArticleMetadata>> GetArticlesAsync(DateTime startDate, DateTime endDate)
        {
            var list = Articles.FindAll(a =>
                (a.PublishDate >= startDate) && (a.PublishDate <= endDate)
                ).ConvertAll(a => a.Metadata);
            list.Sort();
            IEnumerable<ArticleMetadata> ienum = list;
            return Task.FromResult(ienum);
        }

        public Task<IEnumerable<ArticleMetadata>> GetLastArticlesAsync(int numberOfArticles)
        {
            var list = Articles.ConvertAll(a => a.Metadata);
            list.Sort();
            list.Reverse();
            IEnumerable<ArticleMetadata> ienum = list.GetRange(0, numberOfArticles);
            return Task.FromResult(ienum);
        }

        public Task<IEnumerable<ArticleComment>> GetArticleComments(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
        {
            var comments = _GetCommentsDictionaryForArticle(articleSlug).Where(ac => ac.TimestampId.CompareTo(sinceTimestamp) > 0);
            return Task.FromResult(latest ? comments.Reverse().Take(pageSize) : comments.Take(pageSize));
        }

        public Task<bool> AddComment(string articleSlug, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            var result = _GetCommentsDictionaryForArticle(articleSlug).Add(new ArticleComment
            {
                ArticleSlug = articleSlug,
                TimestampId = ArticleComment.TimestampPlusReplyTo(timestamp, replyingTo),
                Content = content,
                AuthorId = userId,
                AuthorName = userName,
            });

            return Task.FromResult(result);
        }

        private SortedSet<ArticleComment> _GetCommentsDictionaryForArticle(string articleSlug)
        {
            SortedSet<ArticleComment> comments;
            lock (_articleComments)
            {
                if (!_articleComments.TryGetValue(articleSlug, out comments))
                {
                    comments = new SortedSet<ArticleComment>();
                    _articleComments.Add(articleSlug, comments);
                }
            }
            return comments;
        }
    }
}
