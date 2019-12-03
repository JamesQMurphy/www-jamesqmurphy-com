using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class ArticleComment : IComparable<ArticleComment>
    {
        public readonly SortedSet<ArticleComment> Replies = new SortedSet<ArticleComment>();
        public string Id { get; set; }
        public DateTime PublishDate { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public ArticleComment ReplyingTo { get; set; }
        public int CompareTo(ArticleComment other) => PublishDate.CompareTo(other.PublishDate);
    }
}
