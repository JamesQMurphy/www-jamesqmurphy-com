using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class Article
    {
        private const string yamlHeader = "---";
        private const string yamlFooter = "...";

        public ArticleMetadata Metadata { get; set; } = new ArticleMetadata();
        public string Content { get; set; }

        // Project metadata fields
        public string Title { get => Metadata.Title; set => Metadata.Title = value; }
        public string Slug { get => Metadata.Slug; set => Metadata.Slug = value; }
        public DateTime PublishDate { get => Metadata.PublishDate; set => Metadata.PublishDate = value; }
        public string Description { get => Metadata.Description; set => Metadata.Description = value; }
        public string MonthString { get => Metadata.MonthString; }
        public string YearString { get => Metadata.YearString; }

        public static async Task<Article> ReadFromAsync(TextReader reader)
        {
            var article = new Article();
            var sbContent = new StringBuilder();

            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                switch (line)
                {
                    case yamlHeader:
                        article.Metadata = ArticleMetadata.ReadFrom(reader);
                        break;

                    case yamlFooter:
                        break;

                    default:
                        sbContent.AppendLine(line);
                        break;
                }
                line = await reader.ReadLineAsync();
            }
            article.Content = sbContent.ToString();
            return article;
        }
        public static Article ReadFrom(TextReader reader)
        {
            return ReadFromAsync(reader).GetAwaiter().GetResult();
        }

        public static async Task<Article> ReadFromAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return await ReadFromAsync(reader);
            }
        }
        public static Article ReadFrom(Stream stream)
        {
            return ReadFromAsync(stream).GetAwaiter().GetResult();
        }

        public static async Task<Article> ReadFromAsync(String s)
        {
            using (var reader = new StringReader(s))
            {
                return await ReadFromAsync(reader);
            }
        }
        public static Article ReadFrom(String s)
        {
            return ReadFromAsync(s).GetAwaiter().GetResult();
        }


        public async Task WriteToAsync(TextWriter writer)
        {
            await Metadata.WriteToAsync(writer);
            await writer.WriteAsync(Content);
        }

        public async void WriteTo(TextWriter writer)
        {
            await WriteToAsync(writer);
        }

        public async void WriteTo(Stream stream, bool leaveOpen = true)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1, leaveOpen))
            {
                await WriteToAsync(writer);
            }
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }

        public override int GetHashCode()
        {
            return Metadata.GetHashCode() ^ Content.GetHashCode();
        }
    }
}
