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
                    Title = "Article Two",
                    Slug = "article-two",
                    PublishDate = new DateTime(2019, 1, 10, 14, 57, 32),
                    Content = "This is article two, published on January 10, 2019 at 2:57pm UTC"
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

        [Test]
        public void GetArticlesByYear()
        {
            for (int year = 2015; year < 2025; year++)
            {
                var articlesThisYear = Store.Articles.FindAll(a => a.PublishDate.Year == year);
                articlesThisYear.Sort((a1, a2) => (a1.PublishDate.CompareTo(a2.PublishDate)));

                var returnedArticleMetadatas = Store.GetArticles(yearString: year.ToString());
                var returnedArticleMetadatasList = new List<ArticleMetadata>(returnedArticleMetadatas);
                returnedArticleMetadatasList.Sort((a1, a2) => (a1.PublishDate.CompareTo(a2.PublishDate)));

                Assert.AreEqual(articlesThisYear.Count, returnedArticleMetadatasList.Count);
                for (int i = 0; i < articlesThisYear.Count; i++)
                {
                    Assert.AreEqual(articlesThisYear[i].Metadata, returnedArticleMetadatasList[i]);
                }
            }
        }


    }
}