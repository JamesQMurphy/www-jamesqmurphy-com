using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    interface IArticleStore
    {
        Article GetArticle(string yearString, string monthString, string slug);
        IEnumerable<ArticleMetadata> GetArticles(string yearString = null, string monthString = null);
    }
}
