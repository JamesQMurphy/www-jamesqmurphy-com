using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public interface IArticleStore
    {
        Task<Article> GetArticleAsync(string slug);
        Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles);
        Task<IEnumerable<ArticleReaction>> GetArticleComments(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false);
        Task<bool> AddComment(string articleSlug, string content, string userId, string userName, DateTime timestamp, string replyingTo = "");
    }
}
