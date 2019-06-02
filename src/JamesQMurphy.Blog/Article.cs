using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class Article
    {
        private const string titleFieldName = "title";
        private const string slugFieldName = "slug";
        private const string publishDateFieldName = "publish-date";
        private const string yamlHeader = "---";
        private const string yamlFooter = "...";

        public Article()
        {
        }

        public string Title { get; set; }
        public string Slug { get; set; }
        public DateTime PublishDate { get; set; }
        public string Content { get; set; }

        public static async Task<Article> ReadFromAsync(TextReader reader)
        {
            var article = new Article();
            var sbContent = new StringBuilder();
            bool bInsideYaml = false;

            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                switch (line)
                {
                    case yamlHeader:
                        bInsideYaml = !bInsideYaml;
                        break;

                    case yamlFooter:
                        bInsideYaml = false;
                        sbContent.Append(await reader.ReadToEndAsync());
                        break;

                    default:
                        if (bInsideYaml)
                        {
                            var match = Regex.Match(line, @"(.+?):(.*)");
                            if (match.Success)
                            {
                                var key = match.Groups[1].Value.Trim().ToLowerInvariant();
                                var value = match.Groups[2].Value.Trim();
                                switch (key)
                                {
                                    case titleFieldName:
                                        article.Title = value;
                                        break;

                                    case slugFieldName:
                                        article.Slug = value;
                                        break;

                                    case publishDateFieldName:
                                        article.PublishDate = DateTime.Parse(value).ToUniversalTime();
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            sbContent.AppendLine(line);
                        }
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
            await writer.WriteLineAsync(yamlHeader);
            await writer.WriteLineAsync($"{titleFieldName}: {Title}");
            await writer.WriteLineAsync($"{slugFieldName}: {Slug}");
            await writer.WriteLineAsync($"{publishDateFieldName}: {PublishDate:O}");
            await writer.WriteLineAsync(yamlFooter);
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
            return Title.GetHashCode() ^ Slug.GetHashCode() ^ PublishDate.GetHashCode() ^ Content.GetHashCode();
        }
    }
}
