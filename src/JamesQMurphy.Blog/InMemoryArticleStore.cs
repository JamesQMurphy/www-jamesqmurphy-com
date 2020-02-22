using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class InMemoryArticleStore : IArticleStore
    {
        private readonly SortedDictionary<DateTime, Article> _articlesByDate = new SortedDictionary<DateTime, Article>();
        private readonly SortedDictionary<string, Article> _articlesBySlug = new SortedDictionary<string, Article>();
        private readonly Dictionary<string, SortedSet<ArticleReaction>> _articleComments = new Dictionary<string, SortedSet<ArticleReaction>>();

        public Task<Article> GetArticleAsync(string slug)
        {
            if (_articlesBySlug.TryGetValue(slug, out Article article))
            {
                return Task.FromResult(article);
            }
            else
            {
                return Task.FromResult((Article)null);
            }
        }

        public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(_articlesByDate.Keys
                .Where(d => d >= startDate && d <= endDate)
                .OrderByDescending(d => d)
                .Select(d => _articlesByDate[d].Metadata)
            );
        }

        public Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
        {
            return Task.FromResult(_articlesByDate.Keys
                .Reverse()
                .Take(numberOfArticles)
                .Select(d => _articlesByDate[d])
            );
        }

        public void SafeAddArticle(Article article)
        {
            _articlesByDate[article.PublishDate] = article;
            _articlesBySlug[article.Slug] = article;
        }

        public Task<IEnumerable<ArticleReaction>> GetArticleComments(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
        {
            var comments = _GetCommentsDictionaryForArticle(articleSlug).Where(ac => ac.TimestampString.CompareTo(sinceTimestamp ?? "") > 0);
            return Task.FromResult(latest ? comments.Reverse().Take(pageSize) : comments.Take(pageSize));
        }

        public Task<bool> AddComment(string articleSlug, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            var result = _GetCommentsDictionaryForArticle(articleSlug).Add(new ArticleReaction
            {
                ArticleSlug = articleSlug,
                TimestampId = (new ArticleReactionTimestampId(timestamp, replyingTo)).ToString(),
                Content = content,
                AuthorId = userId,
                AuthorName = userName,
            });

            return Task.FromResult(result);
        }

        private SortedSet<ArticleReaction> _GetCommentsDictionaryForArticle(string articleSlug)
        {
            SortedSet<ArticleReaction> comments;
            lock (_articleComments)
            {
                if (!_articleComments.TryGetValue(articleSlug, out comments))
                {
                    comments = new SortedSet<ArticleReaction>();
                    _articleComments.Add(articleSlug, comments);
                }
            }
            return comments;
        }
    }
}
