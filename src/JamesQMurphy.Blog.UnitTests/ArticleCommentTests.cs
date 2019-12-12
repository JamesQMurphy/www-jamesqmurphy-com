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
            Assert.IsInstanceOf<ArticleComment>(new ArticleComment());
        }

        [Test]
        public void CanSetProperties()
        {
            var content = "Some Title";
            var slug = "Some-Slug";
            var authorId = "abc123";
            var authorName = "SomeUserName";
            var timestampId = "2019-12-07T15:27:47.8710606Z";

            var ArticleComment = new ArticleComment()
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
            var ArticleComment = new ArticleComment()
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
            var originalTime = System.DateTime.UtcNow.AddHours(-1);
            var replyTime = originalTime.AddMinutes(30);

            var idOriginal = originalTime.ToString("O");
            var idReply = replyTime.ToString("O") + "/" + idOriginal;

            Assert.AreEqual(idOriginal, ArticleComment.TimestampPlusReplyTo(originalTime, ""));
            Assert.AreEqual(idReply, ArticleComment.TimestampPlusReplyTo(replyTime, idOriginal));
        }
    }
}