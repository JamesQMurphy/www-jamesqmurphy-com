using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class LocalFolderArticleStore : IArticleStore
    {
        private readonly string RootFolder;

        public LocalFolderArticleStore(string rootFolder)
        {
            RootFolder = rootFolder;
        }

        public Task<Article> GetArticleAsync(string slug)
        {
            return Task.FromResult(LoadArticlesFromFiles()
                .Where(a => a.Slug == slug)
                .FirstOrDefault());
        }

        public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(
                LoadArticlesFromFiles()
                    .Where(a => (a.PublishDate >= startDate) && (a.PublishDate <= endDate))
                    .OrderByDescending(a => a.PublishDate)
                    .Select(a => a.Metadata)
                );
        }

        public Task<IEnumerable<ArticleMetadata>> GetLastArticlesAsync(int numberOfArticles)
        {
            return Task.FromResult(
                LoadArticlesFromFiles()
                    .OrderByDescending(a => a.PublishDate)
                    .Take(numberOfArticles)
                    .Select(a => a.Metadata)
                );
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
