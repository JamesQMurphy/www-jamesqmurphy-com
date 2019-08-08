using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JamesQMurphy.Blog
{
    public class LocalFolderArticleStore : IArticleStore
    {
        private readonly string RootFolder;

        public LocalFolderArticleStore(string rootFolder)
        {
            RootFolder = rootFolder;
        }

        public Article GetArticle(string yearString, string monthString, string slug)
        {
            return LoadArticlesFromFiles()
                .Where(article => (article.YearString == yearString) && (article.MonthString == monthString) && (article.Slug == slug))
                .FirstOrDefault();
        }

        public IEnumerable<ArticleMetadata> GetArticles(string yearString = null, string monthString = null)
        {
            return LoadArticlesFromFiles()
                .Where(article => (yearString is null || article.YearString == yearString) && (monthString is null || article.MonthString == monthString))
                .Select(article => article.Metadata);
        }

        private IEnumerable<Article> LoadArticlesFromFiles()
        {
            foreach (var filename in Directory.GetFiles(RootFolder, "*.md"))
            {
                var article = Article.ReadFrom(File.Open(filename, FileMode.Open));
                yield return article;
            }
        }
    }
}
