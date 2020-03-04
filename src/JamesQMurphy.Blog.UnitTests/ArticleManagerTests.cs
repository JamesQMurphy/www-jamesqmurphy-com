using JamesQMurphy.Blog;
using NUnit.Framework;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests
{
    public class ArticleManagerTests
    {
        private Article _article = new Article
        {
            Title = "Article One",
            Slug = "2019/01/article-one",
            PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
            Content = "This is article one, published on January 10, 2019 at 12:34pm UTC"
        };
        private InMemoryArticleStore _store = new InMemoryArticleStore();
        private ArticleManager _articleManager;

        [SetUp]
        public void Setup()
        {
            _store.SafeAddArticle(_article);
            _articleManager = new ArticleManager(_store);
        }

        [Test]
        public void GetCompleteComment_NoComments()
        {
            var completeComment = _articleManager.GetCompleteComment(_article.Slug, new ArticleReactionTimestampId(DateTime.UtcNow)).GetAwaiter().GetResult();
            Assert.IsNull(completeComment);
        }

        [Test]
        public void GetCompleteComment_WrongSlug()
        {
            var commentReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Comment, "some comment", "userId", "username", DateTime.UtcNow).GetAwaiter().GetResult();
            var completeComment = _articleManager.GetCompleteComment(_article.Slug + "X", new ArticleReactionTimestampId(commentReactionId)).GetAwaiter().GetResult();
            Assert.IsNull(completeComment);
        }

        [Test]
        public void GetCompleteComment_OneComment()
        {
            var content = "some comment";
            var userId = "some_userid";
            var userName = "SomeUserName";
            var timestamp = DateTime.UtcNow;
            var commentReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Comment, content, userId, userName, timestamp).GetAwaiter().GetResult();
            var completeComment = _articleManager.GetCompleteComment(_article.Slug, new ArticleReactionTimestampId(commentReactionId)).GetAwaiter().GetResult();
            Assert.AreEqual(timestamp.ToString("O"), completeComment.TimestampString);
            Assert.AreEqual(content, completeComment.Content);
            Assert.AreEqual(userId, completeComment.AuthorId);
            Assert.AreEqual(userName, completeComment.AuthorName);
            Assert.AreEqual(commentReactionId, completeComment.ReactionId);
            //TODO: Edit State should be empty
        }

        [Test]
        public void GetCompleteComment_OneCommentWithEdit()
        {
            var content = "some comment";
            var userId = "some_userid";
            var userName = "SomeUserName";
            var timestamp = DateTime.UtcNow;
            var commentReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Comment, content, userId, userName, timestamp).GetAwaiter().GetResult();

            var revisedContent = "revised content";
            var timestamp2 = timestamp.AddSeconds(1);
            var editReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Edit, revisedContent, userId, userName, timestamp2, commentReactionId).GetAwaiter().GetResult();

            var completeComment = _articleManager.GetCompleteComment(_article.Slug, new ArticleReactionTimestampId(commentReactionId)).GetAwaiter().GetResult();
            Assert.AreEqual(timestamp.ToString("O"), completeComment.TimestampString);
            Assert.AreEqual(revisedContent, completeComment.Content);
            Assert.AreEqual(userId, completeComment.AuthorId);
            Assert.AreEqual(userName, completeComment.AuthorName);
            Assert.AreEqual(commentReactionId, completeComment.ReactionId);
            Assert.AreEqual(ArticleReactionEditState.Edited, completeComment.EditState);
        }

        [Test]
        public void GetCompleteComment_OneCommentWithHide()
        {
            var content = "some comment";
            var userId = "some_userid";
            var userName = "SomeUserName";
            var timestamp = DateTime.UtcNow;
            var commentReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Comment, content, userId, userName, timestamp).GetAwaiter().GetResult();

            var timestamp2 = timestamp.AddSeconds(1);
            var editReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Hide, "doesn't matter", userId, userName, timestamp2, commentReactionId).GetAwaiter().GetResult();

            var completeComment = _articleManager.GetCompleteComment(_article.Slug, new ArticleReactionTimestampId(commentReactionId)).GetAwaiter().GetResult();
            Assert.AreEqual(timestamp.ToString("O"), completeComment.TimestampString);
            Assert.AreEqual(content, completeComment.Content);
            Assert.AreEqual(userId, completeComment.AuthorId);
            Assert.AreEqual(userName, completeComment.AuthorName);
            Assert.AreEqual(commentReactionId, completeComment.ReactionId);
            Assert.AreEqual(ArticleReactionEditState.Hidden, completeComment.EditState);
        }

        [Test]
        public void GetCompleteComment_OneCommentWithDelete()
        {
            var content = "some comment";
            var userId = "some_userid";
            var userName = "SomeUserName";
            var timestamp = DateTime.UtcNow;
            var commentReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Comment, content, userId, userName, timestamp).GetAwaiter().GetResult();

            var timestamp2 = timestamp.AddSeconds(1);
            var editReactionId = _store.AddReaction(_article.Slug, ArticleReactionType.Delete, "doesn't matter", userId, userName, timestamp2, commentReactionId).GetAwaiter().GetResult();

            var completeComment = _articleManager.GetCompleteComment(_article.Slug, new ArticleReactionTimestampId(commentReactionId)).GetAwaiter().GetResult();
            Assert.AreEqual(timestamp.ToString("O"), completeComment.TimestampString);
            Assert.AreEqual(String.Empty, completeComment.Content);
            Assert.AreEqual(userId, completeComment.AuthorId);
            Assert.AreEqual(userName, completeComment.AuthorName);
            Assert.AreEqual(commentReactionId, completeComment.ReactionId);
            Assert.AreEqual(ArticleReactionEditState.Deleted, completeComment.EditState);
        }

        // TODO: add show command

        // TODO: test case where closed/hidden by another



    }
}