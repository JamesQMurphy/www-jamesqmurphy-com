using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class CachedArticleStore<TBackingStore> : IArticleStore where TBackingStore : IArticleStore
    {
        private readonly TBackingStore _backingArticleStore;
        private readonly Dictionary<string, Article> _dictCachedArticles = new Dictionary<string, Article>();
        private readonly SortedList<DateTime, ArticleMetadata> _dictCachedArticleMetadatas = new SortedList<DateTime, ArticleMetadata>();
        private DateTime _cachedStartDate = DateTime.MaxValue;
        private DateTime _cachedEndDate = DateTime.MinValue;
        private List<Article> _lastArticles = new List<Article>();

        public CachedArticleStore(TBackingStore articleStore)
        {
            _backingArticleStore = articleStore;
        }

        public async Task<Article> GetArticleAsync(string slug)
        {
            if (_dictCachedArticles.ContainsKey(slug))
            {
                return _dictCachedArticles[slug];
            }
            var article = await _backingArticleStore.GetArticleAsync(slug);
            _dictCachedArticles[slug] = article;
            return article;
        }

        public async Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("startDate cannot be greater than endDate");
            }

            // If requested range is wholly contained within the cached range, use the cached range
            if ((_dictCachedArticleMetadatas.Count > 0) &&
                (startDate >= _cachedStartDate) &&
                (endDate <= _cachedEndDate))
            {
                return _dictCachedArticleMetadatas.Values
                    .Where(m => m.PublishDate >= startDate && m.PublishDate < endDate)
                    .OrderByDescending(m => m.PublishDate);
            }

            var metadatas = await _backingArticleStore.GetArticleMetadatasAsync(startDate, endDate);

            // If requested range overlaps the cached range, or if there's nothing currently cached,
            // add the returned article metadats into the cache and update the cached dates
            if ((_dictCachedArticleMetadatas.Count == 0) ||
                ((startDate < _cachedEndDate) && (endDate > _cachedEndDate)) ||
                ((startDate < _cachedStartDate) && (endDate > _cachedStartDate)))
            {
                foreach (var metadata in metadatas)
                {
                    _dictCachedArticleMetadatas[metadata.PublishDate] = metadata;
                }
                if (startDate < _cachedStartDate)
                {
                    _cachedStartDate = startDate;
                }
                if (endDate > _cachedEndDate)
                {
                    _cachedEndDate = endDate;
                }
            }

            return metadatas;
        }

        public async Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
        {
            if (_lastArticles.Count < numberOfArticles)
            {
                _lastArticles = new List<Article>(await _backingArticleStore.GetLastArticlesAsync(numberOfArticles));
                foreach (var article in _lastArticles)
                {
                    _dictCachedArticles[article.Slug] = article;
                }
            }
            return _lastArticles.AsEnumerable();
        }

        public async Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
        {
            // TODO: possibly cache reactions
            return await _backingArticleStore.GetArticleReactions(articleSlug, sinceTimestamp, pageSize, latest);
        }

        public async Task<bool> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            return await _backingArticleStore.AddReaction(articleSlug, articleReactionType, content, userId, userName, timestamp, replyingTo);
        }
    }
}
