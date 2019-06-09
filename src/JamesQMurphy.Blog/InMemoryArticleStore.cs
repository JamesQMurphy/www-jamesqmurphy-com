using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class InMemoryArticleStore : IArticleStore
    {
        public readonly List<Article> Articles = new List<Article>();

        public Article GetArticle(string yearString, string monthString, string slug)
        {
            return Articles.FindLast(a => (a.Slug == slug) && (a.YearString == yearString) && (a.MonthString == monthString));
        }

        public IEnumerable<ArticleMetadata> GetArticles(string yearString = null, string monthString = null)
        {
            var list = Articles.FindAll(a =>
                (yearString == null || yearString == a.YearString) &&
                (monthString == null || monthString == a.MonthString)
                ).ConvertAll(a => a.Metadata);
            list.Sort();
            return list;
        }
    }
}
