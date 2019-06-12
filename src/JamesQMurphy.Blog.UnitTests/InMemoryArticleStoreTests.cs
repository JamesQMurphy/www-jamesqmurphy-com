using NUnit.Framework;
using JamesQMurphy.Blog;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class InMemoryArticleStoreTests
    {
        private InMemoryArticleStore Store = new InMemoryArticleStore();

        [SetUp]
        public void Setup()
        {
            Store.Articles.AddRange(new Article[]
            {
                new Article()
                {
                    Title = "Article One",
                    Slug = "article-one",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is article one, published on January 10, 2019 at 12:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Three",
                    Slug = "article-three",
                    PublishDate = new DateTime(2019, 7, 6, 18, 34, 56),
                    Content = "This is article three, published on January 10, 2019 at 6:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Two",
                    Slug = "article-two",
                    PublishDate = new DateTime(2019, 1, 10, 14, 57, 32),
                    Content = "This is article two, published on January 10, 2019 at 2:57pm UTC"
                },

                new Article()
                {
                    Title = "Older Article",
                    Slug = "older-article",
                    PublishDate = new DateTime(2018, 7, 30, 10, 2, 0),
                    Content = "This is an older article from the previous year (2018)"
                },

                new Article()
                {
                    Title = "Article Four",
                    Slug = "article-four",
                    PublishDate = new DateTime(2019, 8, 31, 10, 2, 0),
                    Content = "This is article two, published on August 31, 2019 at 10:02am UTC"
                },


            });
        }

        [Test]
        public void GetSingleArtcle()
        {
            foreach (Article article in Store.Articles)
            {
                Assert.AreEqual(article.Metadata, Store.GetArticle(article.YearString, article.MonthString, article.Slug).Metadata);
            }
        }

        [Test]
        public void GetSingleArticleReturnsNull()
        {
            Assert.IsNull(Store.GetArticle("0000", "00", "not-a-slug"));
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
                var articlesThisYear = Store.Articles.FindAll(a => a.PublishDate.Year == year).ConvertAll(a => a.Metadata);
                articlesThisYear.Sort((a1, a2) => (a1.PublishDate.CompareTo(a2.PublishDate)));

                // Get articles thru service.  They should be sorted
                var returnedArticles = Store.GetArticles(yearString: year.ToString());
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
                    // Manually get articles and sort them
                    var articlesThisYear = Store.Articles.FindAll(a => (a.PublishDate.Year == year) && (a.PublishDate.Month == month)).ConvertAll(a => a.Metadata);
                    articlesThisYear.Sort((a1, a2) => (a1.PublishDate.CompareTo(a2.PublishDate)));

                    // Get articles thru service.  They should be sorted
                    var returnedArticles = Store.GetArticles(yearString: year.ToString(), monthString:month.ToString("D2"));
                    var returnedArticlesList = new List<ArticleMetadata>(returnedArticles);

                    AssertArticleListsAreEqual(articlesThisYear, returnedArticlesList);
                }
            }
        }

        [Test]
        public void GetAllArticles()
        {
            // Manually get articles and sort them
            var articlesThisYear = Store.Articles.ConvertAll(a => a.Metadata);
            articlesThisYear.Sort((a1, a2) => (a1.PublishDate.CompareTo(a2.PublishDate)));

            // Get articles thru service.  They should be sorted
            var returnedArticles = Store.GetArticles();
            var returnedArticlesList = new List<ArticleMetadata>(returnedArticles);

            AssertArticleListsAreEqual(articlesThisYear, returnedArticlesList);
        }

    }
}