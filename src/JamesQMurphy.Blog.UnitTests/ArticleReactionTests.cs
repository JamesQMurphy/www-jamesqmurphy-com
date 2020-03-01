using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests
{
    public class ArticleReactionTests
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

            var articleReaction = new ArticleReaction()
            {
                ArticleSlug = slug,
                Content = content,
                AuthorId = authorId,
                AuthorName = authorName,
                TimestampId = timestampId
            };

            Assert.AreEqual(slug, articleReaction.ArticleSlug);
            Assert.AreEqual(content, articleReaction.Content);
            Assert.AreEqual(authorId, articleReaction.AuthorId);
            Assert.AreEqual(authorName, articleReaction.AuthorName);
            Assert.AreEqual(timestampId, articleReaction.TimestampId);
        }

        [Test]
        public void SettingPropertyToNullForcesEmpty()
        {
            var articleReaction = new ArticleReaction()
            {
                ArticleSlug = null,
                Content = null,
                AuthorId = null,
                AuthorName = null,
                TimestampId = null
            };

            Assert.AreEqual(string.Empty, articleReaction.ArticleSlug);
            Assert.AreEqual(string.Empty, articleReaction.Content);
            Assert.AreEqual(string.Empty, articleReaction.AuthorId);
            Assert.AreEqual(string.Empty, articleReaction.AuthorName);
            Assert.AreEqual(string.Empty, articleReaction.TimestampId);
        }

        [Test]
        public void TimestampParsing()
        {
            var originalTime = new System.DateTime(2019, 10, 15, 8, 30, 0).ToUniversalTime();
            var originalTimestamp = originalTime.ToString("O");
            var originalTimestampId = originalTimestamp;
            var originalCommentId = ArticleReactionTimestampId.TimestampToJQueryFriendlyString(originalTimestamp);
            var originalComment = new ArticleReaction()
            {
                TimestampId = originalTimestampId
            };

            Assert.AreEqual(originalTime, originalComment.PublishDate);
            Assert.AreEqual(originalTimestamp, originalComment.TimestampString);
            Assert.AreEqual(originalCommentId, originalComment.ReactionId);
            Assert.AreEqual(originalTimestampId, originalComment.TimestampId);
            Assert.IsEmpty(originalComment.ReactingToId);

            var replyTime = originalTime.AddMinutes(30);
            var replyTimestamp = replyTime.ToString("O");
            var replyCommentId = originalCommentId + "_" + ArticleReactionTimestampId.TimestampToJQueryFriendlyString(replyTimestamp);
            var replyTimestampId = $"{replyTimestamp}_{originalTimestamp}";
            var replyComment = new ArticleReaction()
            {
                TimestampId = replyTimestampId
            };

            Assert.AreEqual(replyTime, replyComment.PublishDate);
            Assert.AreEqual(replyTimestamp, replyComment.TimestampString);
            Assert.AreEqual(replyCommentId, replyComment.ReactionId);
            Assert.AreEqual(replyTimestampId, replyComment.TimestampId);
            Assert.AreEqual(originalComment.ReactionId, replyComment.ReactingToId);
        }

        [Test]
        public void CompareToNull()
        {
            Assert.AreEqual(0, new ArticleReaction().CompareTo(new ArticleReaction()));
        }

        [Test]
        public void DetermineParentFromId()
        {
            var rootDate = System.DateTime.UtcNow;
            var id1 = new ArticleReactionTimestampId(rootDate);
            var id2 = new ArticleReactionTimestampId(rootDate.AddDays(1), id1.ReactionId);
            var id3 = new ArticleReactionTimestampId(rootDate.AddDays(2), id2.ReactionId);
            var id4 = new ArticleReactionTimestampId(rootDate.AddDays(3), id3.ReactionId);

            Assert.IsEmpty(id1.ReactingToId);
            Assert.AreEqual(1, id1.NestingLevel);
            Assert.AreEqual(id1.ReactionId, id2.ReactingToId);
            Assert.AreEqual(2, id2.NestingLevel);
            Assert.AreEqual(id2.ReactionId, id3.ReactingToId);
            Assert.AreEqual(3, id3.NestingLevel);
            Assert.AreEqual(id3.ReactionId, id4.ReactingToId);
            Assert.AreEqual(4, id4.NestingLevel);
        }

        [Test]
        public void EmptyReactionTimestampIdWorks()
        {
            var reactionTimestampId = new ArticleReactionTimestampId("");
            Assert.IsEmpty(reactionTimestampId.ReactionId);
            Assert.AreEqual(0, reactionTimestampId.NestingLevel);
        }

        [Test]
        public void ConvertToJQueryFriendlyAndBack()
        {
            var timestampString = System.DateTime.UtcNow.ToString("O");

            var jqueryFriendlyString = ArticleReactionTimestampId.TimestampToJQueryFriendlyString(timestampString);

            // from https://stackoverflow.com/a/2837646/1001100
            Assert.IsFalse(Regex.IsMatch(jqueryFriendlyString, @"[!""#$%&'()*+,.\/:;<=>?@[\\\]^`{|}~]"));

            var backToTimestampString = ArticleReactionTimestampId.JQueryFriendlyToTimestampString(jqueryFriendlyString);
            Assert.AreEqual(timestampString, backToTimestampString);
        }

        [Test]
        public void TimestampIdConvertedProperly()
        {
            var rootDate = System.DateTime.UtcNow;
            var replyDate = rootDate.AddDays(1);

            var rootId = new ArticleReactionTimestampId(rootDate);
            var replyId = new ArticleReactionTimestampId(replyDate, rootId.ReactionId);

            // Assert ToString() returns as timestamps
            Assert.AreEqual(replyId.ToString(), ArticleReactionTimestampId.JQueryFriendlyToTimestampString(replyId.ToString()));

            // Assert ReactionId returns as JQuery-friendly
            Assert.AreEqual(replyId.ReactionId, ArticleReactionTimestampId.TimestampToJQueryFriendlyString(replyId.ReactionId));

            // Assert reactionID gets converted back to timestamps internally
            var replyIdFromPrevious = new ArticleReactionTimestampId(replyId.ReactionId);
            Assert.AreEqual(replyIdFromPrevious.ToString(), ArticleReactionTimestampId.JQueryFriendlyToTimestampString(replyIdFromPrevious.ToString()));
        }

        [Test]
        public void ReactingToIdIsCorrerct()
        {
            var timestampId = "2020-02-28T21:41:06.1781051Z_2019-12-26T10:12:00.6799989Z";
            var reactionId = "2019-12-26T10c12c00p6799989Z_2020-02-28T21c41c06p1781051Z";
            var reactingToId = "2019-12-26T10c12c00p6799989Z";

            var comment = new ArticleReaction
            {
                TimestampId = timestampId
            };

            Assert.AreEqual(reactionId, comment.ReactionId);
            Assert.AreEqual(reactingToId, comment.ReactingToId);

            var computed = new ArticleReactionTimestampId(reactionId);
            Assert.AreEqual(reactionId, computed.ReactionId);
            Assert.AreEqual(reactingToId, computed.ReactingToId);

        }

    }
}