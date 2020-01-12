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
    }
}
