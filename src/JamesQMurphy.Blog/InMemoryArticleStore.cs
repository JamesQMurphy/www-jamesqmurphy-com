using System;
using System.Collections.Generic;

namespace JamesQMurphy.Blog
{
    public class InMemoryArticleStore : IArticleStore
    {
        public readonly List<Article> Articles = new List<Article>();

        public Article GetArticle(string slug)
        {
            return Articles.FindLast(a => a.Slug == slug);
        }

        public IEnumerable<ArticleMetadata> GetArticles(DateTime startDate, DateTime endDate)
        {
            var list = Articles.FindAll(a =>
                (a.PublishDate >= startDate) && (a.PublishDate <= endDate)
                ).ConvertAll(a => a.Metadata);
            list.Sort();
            return list;
        }

        public IEnumerable<ArticleMetadata> GetLastArticles(int numberOfArticles)
        {
            var list = Articles.ConvertAll(a => a.Metadata);
            list.Sort();
            list.Reverse();
            return list.GetRange(0, numberOfArticles);
        }
    }
}
