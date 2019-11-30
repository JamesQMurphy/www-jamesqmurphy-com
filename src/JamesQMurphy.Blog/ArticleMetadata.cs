using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public class ArticleMetadata : IComparable<ArticleMetadata>, IEquatable<ArticleMetadata>
    {
        private const string titleFieldName = "title";
        private const string slugFieldName = "slug";
        private const string publishDateFieldName = "publish-date";
        private const string descriptionFieldName = "description";
        private const string yamlHeader = "---";
        private const string yamlFooter = "...";

        private string _title = string.Empty;
        private string _slug = string.Empty;
        private string _description = string.Empty;

        public string Title { get => _title; set => _title = value ?? string.Empty; }
        public string Slug { get => _slug; set => _slug = value ?? string.Empty; }
        public string Description { get => _description; set => _description = value ?? string.Empty; }
        public DateTime PublishDate { get; set; }

        public static async Task<ArticleMetadata> ReadFromAsync(TextReader reader)
        {
            var articleMetadata = new ArticleMetadata();
            bool bPropertiesRead = false;
            bool bDone = false;
            var line = await reader.ReadLineAsync();
            while ((line != null) && (!bDone))
            {
                switch (line)
                {
                    case yamlHeader:
                        // If we haven't read any properties yet, assume that
                        // this is the opening header.  Otherwise, assume it's
                        // the start of a new document and treat it like a footer
                        if (!bPropertiesRead)
                            break;
                        else
                            goto case yamlFooter;

                    case yamlFooter:
                        bDone = true;
                        break;

                    default:
                        var match = Regex.Match(line, @"(.+?):(.*)");
                        if (match.Success)
                        {
                            bPropertiesRead = true;
                            var key = match.Groups[1].Value.Trim().ToLowerInvariant();
                            var value = match.Groups[2].Value.Trim();
                            switch (key)
                            {
                                case titleFieldName:
                                    articleMetadata.Title = value;
                                    break;

                                case slugFieldName:
                                    articleMetadata.Slug = value;
                                    break;

                                case publishDateFieldName:
                                    articleMetadata.PublishDate = DateTime.Parse(value).ToUniversalTime();
                                    break;

                                case descriptionFieldName:
                                    articleMetadata.Description = value;
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;
                }
                if (!bDone)
                {
                    line = await reader.ReadLineAsync();
                }
            }
            return articleMetadata;
        }
        public static ArticleMetadata ReadFrom(TextReader reader)
        {
            return ReadFromAsync(reader).GetAwaiter().GetResult();
        }

        public static async Task<ArticleMetadata> ReadFromAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return await ReadFromAsync(reader);
            }
        }
        public static ArticleMetadata ReadFrom(Stream stream)
        {
            return ReadFromAsync(stream).GetAwaiter().GetResult();
        }

        public static async Task<ArticleMetadata> ReadFromAsync(String s)
        {
            using (var reader = new StringReader(s))
            {
                return await ReadFromAsync(reader);
            }
        }
        public static ArticleMetadata ReadFrom(String s)
        {
            return ReadFromAsync(s).GetAwaiter().GetResult();
        }


        public async Task WriteToAsync(TextWriter writer)
        {
            await writer.WriteLineAsync(yamlHeader);
            await writer.WriteLineAsync($"{titleFieldName}: {Title}");
            await writer.WriteLineAsync($"{slugFieldName}: {Slug}");
            await writer.WriteLineAsync($"{publishDateFieldName}: {PublishDate:O}");
            if (Description.Length > 0)
            {
                await writer.WriteLineAsync($"{descriptionFieldName}: {Description}");
            }
            await writer.WriteLineAsync(yamlFooter);
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ArticleMetadata);
        }

        public bool Equals(ArticleMetadata other)
        {
            return (CompareTo(other) == 0);
        }

        public static bool operator ==(ArticleMetadata left, ArticleMetadata right)
        {
            if (object.ReferenceEquals(left, right)) return true;
            if ((left is null) || (right is null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ArticleMetadata left, ArticleMetadata right)
        {
            if (object.ReferenceEquals(left, right)) return false;
            if ((left is null) || (right is null)) return true;
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode() ^ Slug.GetHashCode() ^ PublishDate.GetHashCode() ^ Description.GetHashCode();
        }

        public override string ToString()
        {
            var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }

        public int CompareTo(ArticleMetadata other)
        {
            if (other == null) return 1;
            if (ReferenceEquals(this, other)) return 0;

            int retval = this.PublishDate.CompareTo(other.PublishDate);
            if (retval == 0)
            {
                retval = this.Title.CompareTo(other.Title);
            }
            if (retval == 0)
            {
                retval = this.Slug.CompareTo(other.Slug);
            }
            if (retval == 0)
            {
                retval = this.Description.CompareTo(other.Description);
            }
            return retval;
        }

        public static bool operator <(ArticleMetadata left, ArticleMetadata right)
        {
            if (object.ReferenceEquals(left, right)) return false;
            if (left is null) return true;
            return left.CompareTo(right) == -1;
        }

        public static bool operator >(ArticleMetadata left, ArticleMetadata right)
        {
            if (left is null) return false;
            return left.CompareTo(right) == 1;
        }

        public static bool operator <=(ArticleMetadata left, ArticleMetadata right)
        {
            return (left < right) || (left == right);
        }

        public static bool operator >=(ArticleMetadata left, ArticleMetadata right)
        {
            return (left > right) || (left == right);
        }

    }
}
