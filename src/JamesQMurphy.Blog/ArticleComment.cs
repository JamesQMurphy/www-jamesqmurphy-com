using System;

namespace JamesQMurphy.Blog
{
    public class ArticleComment : IComparable<ArticleComment>
    {
        private const char TIMESTAMP_DELIMETER = '/';
        private static readonly char[] _timestampSplit = new char[] { TIMESTAMP_DELIMETER };
        public static string TimestampPlusReplyTo(string timestamp, string replyTo)
        {
            return String.IsNullOrWhiteSpace(replyTo) ? timestamp : $"{timestamp}{TIMESTAMP_DELIMETER}{replyTo}";
        }

        private string _articleSlug = string.Empty;
        private string _timestamp = string.Empty;
        private string _authorId = string.Empty;
        private string _authorName = string.Empty;
        private string _content = string.Empty;

        public static string TimestampPlusReplyTo(DateTime timestampUtc, string replyTo)
        {
            return TimestampPlusReplyTo(timestampUtc.ToString("O"), replyTo);
        }
        public string ArticleSlug { get => _articleSlug; set => _articleSlug = value ?? string.Empty; }
        public string Timestamp { get => _timestamp; set => _timestamp = value ?? string.Empty; }
        public string AuthorId { get => _authorId; set => _authorId = value ?? string.Empty; }
        public string AuthorName { get => _authorName; set => _authorName = value ?? string.Empty; }
        public string Content { get => _content; set => _content = value ?? string.Empty; }
        public DateTime PublishDate => DateTime.Parse(Timestamp.Split(_timestampSplit)[0]);
        public int CompareTo(ArticleComment other) => Timestamp.CompareTo(other.Timestamp);
    }
}
