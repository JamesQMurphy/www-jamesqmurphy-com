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
        public async Task<Article> GetArticleAsync(string slug) => await ArticleStore.GetArticleAsync(slug);
        public async Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate) => await ArticleStore.GetArticleMetadatasAsync(startDate, endDate);
        public async Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles) => await ArticleStore.GetLastArticlesAsync(numberOfArticles);
        public async Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false) => await ArticleStore.GetArticleReactions(articleSlug, sinceTimestamp, pageSize, latest);
        public async Task<string> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "") => await ArticleStore.AddReaction(articleSlug, articleReactionType, content, userId, userName, timestamp, replyingTo);

        public async Task<ArticleReaction> GetCompleteComment(string slug, ArticleReactionTimestampId reactionId)
        {
            var timestampJustBefore = reactionId.TimestampAsString.Substring(0, reactionId.TimestampAsString.Length - 1);
            var reactions = (await GetArticleReactions(slug, timestampJustBefore, 0))
                .Where(r => r.ReactionId == reactionId.ReactionId || r.ReactingToId == reactionId.ReactionId)
                .ToList();
            if (reactions.Count == 0 || reactions[0].ReactionType != ArticleReactionType.Comment)
            {
                return null;
            }
            var completeComment = reactions[0];
            for (int i = 1; i < reactions.Count; i++)
            {
                var r = reactions[i];
                switch (r.ReactionType)
                {
                    case ArticleReactionType.Edit:
                        completeComment.Content = r.Content;
                        completeComment.EditState = ArticleReactionEditState.Edited;
                        break;

                    case ArticleReactionType.Hide:
                        completeComment.EditState = ArticleReactionEditState.Hidden;
                        break;

                    case ArticleReactionType.Delete:
                        completeComment.Content = String.Empty;
                        completeComment.EditState = ArticleReactionEditState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            return completeComment;
        }

        public async Task<bool> ValidateReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, bool isAdministrator, string replyingTo = "")
        {
            // Validate that article exists and is not locked (using cached version is okay)
            var article = await ArticleStore.GetArticleAsync(articleSlug);
            if (article == null || article.LockedForComments)
            {
                return false;
            }

            // If reacting to a comment, validate that the comment exists
            if (!String.IsNullOrEmpty(replyingTo))
            {
                var reactingToComment = await GetCompleteComment(articleSlug, new ArticleReactionTimestampId(replyingTo));
                if (reactingToComment == null)
                {
                    return false;
                }
            }

            // Passed validation
            return true;
        }
    }
}
