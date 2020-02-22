using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class LocalFolderArticleStore : IArticleStore
    {
        private const string COMMENT_SEPARATOR = "-------------------------------------------------------------";
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

        public Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
        {
            return Task.FromResult(
                LoadArticlesFromFiles()
                    .OrderByDescending(a => a.PublishDate)
                    .Take(numberOfArticles)
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

        public async Task<IEnumerable<ArticleReaction>> GetArticleComments(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
        {
            var listToReturn = new SortedSet<ArticleReaction>();
            var currentComment = new ArticleReaction();

            void _addIfMatchesCriteria()
            {
                if(currentComment.ArticleSlug == articleSlug)
                {
                    if (string.IsNullOrWhiteSpace(sinceTimestamp) || currentComment.TimestampString.CompareTo(sinceTimestamp) > 0)
                    {
                        listToReturn.Add(currentComment);
                    }
                }
            }

            using (var reader = File.OpenText(Path.Combine(RootFolder, "comments.txt")))
            {
                bool readingContent = false;
                var lineRead = await reader.ReadLineAsync();
                while (lineRead != null)
                {
                    if (lineRead.StartsWith(COMMENT_SEPARATOR))
                    {
                        _addIfMatchesCriteria();
                        currentComment = new ArticleReaction();
                        readingContent = false;
                    }
                    else
                    {
                        if (readingContent)
                        {
                            currentComment.Content += Environment.NewLine + lineRead;
                        }
                        else
                        {
                            currentComment.ArticleSlug = lineRead;
                            currentComment.TimestampId = await reader.ReadLineAsync();
                            currentComment.AuthorId = await reader.ReadLineAsync();
                            currentComment.AuthorName = await reader.ReadLineAsync();
                            currentComment.Content = await reader.ReadLineAsync();
                            readingContent = true;
                        }
                    }
                    lineRead = await reader.ReadLineAsync();
                }
            }
            _addIfMatchesCriteria();
            if (latest)
            {
                return listToReturn.Reverse().Take(pageSize);
            }
            else
            {
                return listToReturn.Take(pageSize);
            }
        }

        public async Task<bool> AddComment(string articleSlug, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            using (var writer = new StreamWriter(Path.Combine(RootFolder, "comments.txt"), true))
            {
                await writer.WriteLineAsync(COMMENT_SEPARATOR);
                await writer.WriteLineAsync(articleSlug);
                await writer.WriteLineAsync((new ArticleReactionTimestampId(timestamp, replyingTo)).ToString());
                await writer.WriteLineAsync(userId);
                await writer.WriteLineAsync(userName);
                await writer.WriteLineAsync(content);
            }
            return true;
        }

    }
}
