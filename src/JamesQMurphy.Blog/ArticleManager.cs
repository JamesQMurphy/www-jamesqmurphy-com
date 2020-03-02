using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class ArticleManager : IArticleStore
    {
        public IArticleStore ArticleStore { get; private set; }
        public ArticleManager(IArticleStore articleStore)
        {
            ArticleStore = articleStore;
        }

        // Project IArticleStore members
        public Task<Article> GetArticleAsync(string slug) => ArticleStore.GetArticleAsync(slug);
        public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate) => ArticleStore.GetArticleMetadatasAsync(startDate, endDate);
        public Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles) => ArticleStore.GetLastArticlesAsync(numberOfArticles);
        public Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false) => ArticleStore.GetArticleReactions(articleSlug, sinceTimestamp, pageSize, latest);
        public Task<string> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "") => ArticleStore.AddReaction(articleSlug, articleReactionType, content, userId, userName, timestamp, replyingTo);

        public async Task<ArticleReaction> GetCompleteComment(string slug, ArticleReactionTimestampId reactionId)
        {
            var timestampJustBefore = reactionId.TimeStampString.Substring(0, reactionId.TimeStampString.Length - 1);
            var reactions = (await GetArticleReactions(slug, timestampJustBefore, 0))
                .Where(r => r.ReactionId == reactionId.ReactionId || r.ReactingToId == reactionId.ReactionId)
                .ToList();
            if (reactions.Count == 0 || reactions[0].ReactionType != ArticleReactionType.Comment)
            {
                return null;
            }
            return reactions[0];
        }

    }
}
