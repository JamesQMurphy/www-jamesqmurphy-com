using System;

namespace JamesQMurphy.Blog
{
    public class ArticleReaction : IComparable<ArticleReaction>
    {
        private ArticleReactionTimestampId _articleCommentTimestampId = null;
        private string _articleSlug = string.Empty;
        private string _authorId = string.Empty;
        private string _authorName = string.Empty;
        private ArticleReactionType _reactionType = ArticleReactionType.Comment;
        private ArticleReactionEditState _editState = ArticleReactionEditState.Original;
        private string _content = string.Empty;

        public string ArticleSlug { get => _articleSlug; set => _articleSlug = value ?? string.Empty; }
        public string TimestampId
        {
            get => _articleCommentTimestampId?.ToString() ?? "";
            set => _articleCommentTimestampId = new ArticleReactionTimestampId(value);
        }
        public string AuthorId { get => _authorId; set => _authorId = value ?? string.Empty; }
        public string AuthorName { get => _authorName; set => _authorName = value ?? string.Empty; }
        public ArticleReactionType ReactionType { get => _reactionType; set => _reactionType = value; }
        public ArticleReactionEditState EditState { get => _editState; set => _editState = value; }
        public string Content { get => _content; set => _content = value ?? string.Empty; }
        public DateTime PublishDate => _articleCommentTimestampId.TimeStamp;
        public string ReactionId => _articleCommentTimestampId.ReactionId;
        public string ReactingToId => _articleCommentTimestampId.ReactingToId;
        public string TimestampString => _articleCommentTimestampId.TimeStampString;
        public int NestingLevel => _articleCommentTimestampId.NestingLevel;
        public int CompareTo(ArticleReaction other) => TimestampId.CompareTo(other.TimestampId);
        public override int GetHashCode() => ReactionId.GetHashCode();
    }
}
