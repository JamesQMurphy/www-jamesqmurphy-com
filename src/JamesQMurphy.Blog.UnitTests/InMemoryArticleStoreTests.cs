using NUnit.Framework;
using JamesQMurphy.Blog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class InMemoryArticleStoreTests
    {
        private InMemoryArticleStore Store = new InMemoryArticleStore();
        private List<Article> _articles = new List<Article>();

        [SetUp]
        public void Setup()
        {
            _articles.Clear();
            _articles.AddRange( new Article[]
            {
                new Article()
                {
                    Title = "Article One",
                    Slug = "2019/01/article-one",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is article one, published on January 10, 2019 at 12:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Three",
                    Slug = "2019/07/article-three",
                    PublishDate = new DateTime(2019, 7, 6, 18, 34, 56),
                    Content = "This is article three, published on January 10, 2019 at 6:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Two",
                    Slug = "2019/01/article-two",
                    PublishDate = new DateTime(2019, 1, 10, 14, 57, 32),
                    Content = "This is article two, published on January 10, 2019 at 2:57pm UTC"
                },

                new Article()
                {
                    Title = "Older Article",
                    Slug = "2018/07/older-article",
                    PublishDate = new DateTime(2018, 7, 30, 10, 2, 0),
                    Content = "This is an older article from the previous year (2018)"
                },

                new Article()
                {
                    Title = "Article Four",
                    Slug = "2019/08/article-four",
                    PublishDate = new DateTime(2019, 8, 31, 10, 2, 0),
                    Content = "This is article two, published on August 31, 2019 at 10:02am UTC"
                },


            });
            Store = new InMemoryArticleStore();
            foreach (var a in _articles)
            {
                Store.SafeAddArticle(a);
            }
        }

        [Test]
        public void GetSingleArticle()
        {
            foreach (Article article in _articles)
            {
                Assert.AreEqual(article.Metadata, Store.GetArticleAsync(article.Slug).GetAwaiter().GetResult().Metadata);
            }
        }

        [Test]
        public void GetSingleArticleReturnsNull()
        {
            Assert.IsNull(Store.GetArticleAsync("0000/00/not-a-slug").GetAwaiter().GetResult());
        }

        private void AssertArticleListsAreEqual(IList<ArticleMetadata> expected, IList<ArticleMetadata> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }


        [Test]
        public void GetArticlesByYear()
        {
            for (int year = 2015; year < 2025; year++)
            {
                // Manually get articles and sort them
                var articlesThisYear = _articles.FindAll(a => a.PublishDate.Year == year).ConvertAll(a => a.Metadata);
                articlesThisYear.Sort((a1, a2) => (a2.PublishDate.CompareTo(a1.PublishDate)));

                // Get articles thru service.  They should be sorted
                var returnedArticles = Store.GetArticleMetadatasAsync(new DateTime(year, 1, 1), new DateTime(year + 1, 1, 1).AddSeconds(-1)).GetAwaiter().GetResult();
                var returnedArticlesList = new List<ArticleMetadata>(returnedArticles);

                AssertArticleListsAreEqual(articlesThisYear, returnedArticlesList);
            }
        }

        [Test]
        public void GetArticlesByYearAndMonth()
        {
            for (int year = 2015; year < 2025; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    // Manually get articles and sort them in decending order
                    var articlesThisYear = _articles.FindAll(a => (a.PublishDate.Year == year) && (a.PublishDate.Month == month)).ConvertAll(a => a.Metadata);
                    articlesThisYear.Sort((a1, a2) => (a2.PublishDate.CompareTo(a1.PublishDate)));

                    // Get articles thru service.  They should be sorted
                    var returnedArticles = Store.GetArticleMetadatasAsync(new DateTime(year, month, 1), new DateTime(year, month, 1).AddMonths(1).AddSeconds(-1)).GetAwaiter().GetResult();
                    var returnedArticlesList = new List<ArticleMetadata>(returnedArticles);

                    AssertArticleListsAreEqual(articlesThisYear, returnedArticlesList);
                }
            }
        }

        [Test]
        public void GetAllArticles()
        {
            // Manually get articles and sort them in decending order
            var allArticleMetadatas = _articles.ConvertAll(a => a.Metadata);
            allArticleMetadatas.Sort((a1, a2) => (a2.PublishDate.CompareTo(a1.PublishDate)));

            // Get articles thru service.  They should be sorted
            var returnedArticles = Store.GetArticleMetadatasAsync(DateTime.MinValue, DateTime.MaxValue).GetAwaiter().GetResult();
            var returnedArticlesList = new List<ArticleMetadata>(returnedArticles);

            AssertArticleListsAreEqual(allArticleMetadatas, returnedArticlesList);
        }

        [Test]
        public void NoArticleReturnsNull()
        {
            Assert.IsNull(Store.GetArticleAsync("doesn't exist").GetAwaiter().GetResult());
        }

        [Test]
        public void NestedComments()
        {
            var rootDate = System.DateTime.UtcNow;
            var article = _articles[0];

            Store.AddReaction(article.Slug, ArticleReactionType.Comment, "content", "userId", "username", rootDate);
            var reaction1 = Store.GetArticleReactions(article.Slug).GetAwaiter().GetResult().First();
            Assert.IsEmpty(reaction1.ReactingToId);
            Assert.AreEqual(1, reaction1.NestingLevel);

            Store.AddReaction(article.Slug, ArticleReactionType.Comment, "content", "userId", "username", rootDate.AddDays(1), reaction1.ReactionId);
            var reaction2 = Store.GetArticleReactions(article.Slug).GetAwaiter().GetResult().Last();
            Assert.AreEqual(reaction1.ReactionId, reaction2.ReactingToId);
            Assert.AreEqual(2, reaction2.NestingLevel);

            Store.AddReaction(article.Slug, ArticleReactionType.Comment, "content", "userId", "username", rootDate.AddDays(2), reaction2.ReactionId);
            var reaction3 = Store.GetArticleReactions(article.Slug).GetAwaiter().GetResult().Last();
            Assert.AreEqual(reaction2.ReactionId, reaction3.ReactingToId);
            Assert.AreEqual(3, reaction3.NestingLevel);
        }

        [Test]
        public void ZeroPagesizeReturnsAllArticles()
        {
            var rootDate = System.DateTime.UtcNow;
            var article = _articles[0];

            Store.AddReaction(article.Slug, ArticleReactionType.Comment, "content", "userId", "username", rootDate);
            var reaction = Store.GetArticleReactions(article.Slug).GetAwaiter().GetResult().First();

            var reactionsReturned = Store.GetArticleReactions(article.Slug, pageSize: 0).GetAwaiter().GetResult().ToList();
            Assert.AreEqual(1, reactionsReturned.Count);
            Assert.AreEqual(reaction.ReactionId, reactionsReturned[0].ReactionId);
        }

    }
}