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

        public Article GetArticle(string slug)
        {
            return LoadArticlesFromFiles()
                .Where(a => a.Slug == slug)
                .FirstOrDefault();
        }

        public IEnumerable<ArticleMetadata> GetArticles(DateTime startDate, DateTime endDate)
        {
            return LoadArticlesFromFiles()
                .Where(a => (a.PublishDate >= startDate) && (a.PublishDate <= endDate))
                .OrderByDescending(a => a.PublishDate)
                .Select(a => a.Metadata);
        }

        public IEnumerable<ArticleMetadata> GetLastArticles(int numberOfArticles)
        {
            return LoadArticlesFromFiles()
                .OrderByDescending(a => a.PublishDate)
                .Take(numberOfArticles)
                .Select(a => a.Metadata);
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
