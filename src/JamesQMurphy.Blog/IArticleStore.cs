using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public interface IArticleStore
    {
        Article GetArticle(string slug);
        IEnumerable<ArticleMetadata> GetArticles(DateTime startDate, DateTime endDate);
        IEnumerable<ArticleMetadata> GetLastArticles(int numberOfArticles);
    }
}
