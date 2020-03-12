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
        Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false);
        Task<string> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "");
    }
}
