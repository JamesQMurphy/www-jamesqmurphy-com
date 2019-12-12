using System;

namespace JamesQMurphy.Blog
{
    public class ArticleComment : IComparable<ArticleComment>
    {
        private ArticleCommentTimestampId _articleCommentTimestampId = null;
        private string _articleSlug = string.Empty;
        private string _authorId = string.Empty;
        private string _authorName = string.Empty;
        private string _content = string.Empty;

        public string ArticleSlug { get => _articleSlug; set => _articleSlug = value ?? string.Empty; }
        public string TimestampId
        {
            get => _articleCommentTimestampId?.ToString() ?? "";
            set => _articleCommentTimestampId = new ArticleCommentTimestampId(value);
        }
        public string AuthorId { get => _authorId; set => _authorId = value ?? string.Empty; }
        public string AuthorName { get => _authorName; set => _authorName = value ?? string.Empty; }
        public string Content { get => _content; set => _content = value ?? string.Empty; }
        public DateTime PublishDate => _articleCommentTimestampId.TimeStamp;
        public string CommentId => _articleCommentTimestampId.CommentId;
        public string ReplyToId => _articleCommentTimestampId.ReplyToId;
        public int CompareTo(ArticleComment other) => TimestampId.CompareTo(other.TimestampId);
    }
}
