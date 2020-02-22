using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;

namespace Tests
{
    public class ArticleCommentTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreate()
        {
            Assert.IsInstanceOf<ArticleReaction>(new ArticleReaction());
        }

        [Test]
        public void CanSetProperties()
        {
            var content = "Some Title";
            var slug = "Some-Slug";
            var authorId = "abc123";
            var authorName = "SomeUserName";
            var timestampId = "2019-12-07T15:27:47.8710606Z";

            var ArticleComment = new ArticleReaction()
            {
                ArticleSlug = slug,
                Content = content,
                AuthorId = authorId,
                AuthorName = authorName,
                TimestampId = timestampId
            };

            Assert.AreEqual(slug, ArticleComment.ArticleSlug);
            Assert.AreEqual(content, ArticleComment.Content);
            Assert.AreEqual(authorId, ArticleComment.AuthorId);
            Assert.AreEqual(authorName, ArticleComment.AuthorName);
            Assert.AreEqual(timestampId, ArticleComment.TimestampId);
        }

        [Test]
        public void SettingPropertyToNullForcesEmpty()
        {
            var ArticleComment = new ArticleReaction()
            {
                ArticleSlug = null,
                Content = null,
                AuthorId = null,
                AuthorName = null,
                TimestampId = null
            };

            Assert.AreEqual(string.Empty, ArticleComment.ArticleSlug);
            Assert.AreEqual(string.Empty, ArticleComment.Content);
            Assert.AreEqual(string.Empty, ArticleComment.AuthorId);
            Assert.AreEqual(string.Empty, ArticleComment.AuthorName);
            Assert.AreEqual(string.Empty, ArticleComment.TimestampId);
        }

        [Test]
        public void TimestampParsing()
        {
            var originalTime = new System.DateTime(2019, 10, 15, 8, 30, 0).ToUniversalTime();
            var originalTimestamp = originalTime.ToString("O");
            var originalTimestampId = originalTimestamp;
            var originalCommentId = originalTimestamp.Replace('.', '-').Replace(':', '-');
            var originalComment = new ArticleReaction()
            {
                TimestampId = originalTimestampId
            };

            Assert.AreEqual(originalTime, originalComment.PublishDate);
            Assert.AreEqual(originalTimestamp, originalComment.TimestampString);
            Assert.AreEqual(originalCommentId, originalComment.CommentId);
            Assert.AreEqual(originalTimestampId, originalComment.TimestampId);
            Assert.IsEmpty(originalComment.ReplyToId);

            var replyTime = originalTime.AddMinutes(30);
            var replyTimestamp = replyTime.ToString("O");
            var replyCommentId = originalCommentId + "_" + replyTimestamp.Replace('.', '-').Replace(':', '-');
            var replyTimestampId = $"{replyTimestamp}_{originalTimestamp}";
            var replyComment = new ArticleReaction()
            {
                TimestampId = replyTimestampId
            };

            Assert.AreEqual(replyTime, replyComment.PublishDate);
            Assert.AreEqual(replyTimestamp, replyComment.TimestampString);
            Assert.AreEqual(replyCommentId, replyComment.CommentId);
            Assert.AreEqual(replyTimestampId, replyComment.TimestampId);
            Assert.AreEqual(originalComment.CommentId, replyComment.ReplyToId);
        }

        [Test]
        public void CompareToNull()
        {
            Assert.AreEqual(0, new ArticleReaction().CompareTo(new ArticleReaction()));
        }
    }
}