using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class InMemoryArticleStore : IArticleStore
    {
        public readonly List<Article> Articles = new List<Article>();

        public Task<Article> GetArticleAsync(string slug)
        {
            return Task.FromResult(Articles.FindLast(a => a.Slug == slug));
        }

        public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
        {
            var list = Articles.FindAll(a =>
                (a.PublishDate >= startDate) && (a.PublishDate <= endDate)
                ).ConvertAll(a => a.Metadata);
            list.Sort();
            IEnumerable<ArticleMetadata> ienum = list;
            return Task.FromResult(ienum);
        }

        public Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
        {
            var list = Articles.ConvertAll(a => a.Metadata);
            list.Sort();
            list.Reverse();
            IEnumerable<Article> ienum = list.GetRange(0, numberOfArticles).ConvertAll(m => Articles.FindLast(a => a.Slug == m.Slug));
            return Task.FromResult(ienum);
        }
    }
}
