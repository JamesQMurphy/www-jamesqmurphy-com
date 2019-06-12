using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public interface IArticleStore
    {
        Article GetArticle(string yearString, string monthString, string slug);
        IEnumerable<ArticleMetadata> GetArticles(string yearString = null, string monthString = null);
    }
}
