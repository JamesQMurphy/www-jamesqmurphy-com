using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace JamesQMurphy.Web.UnitTests
{
    public class HomeControllerTests
    {
        private InMemoryArticleStore articleStore;
        private Controllers.HomeController controller;
        private const string SITE_NAME = "TEST SITE 820ae666";

        [SetUp]
        public void Setup()
        {
            articleStore = new InMemoryArticleStore();
            articleStore.Articles.AddRange(new Article[]
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
                    Content = "This is article three, published on July 6, 2019 at 6:34pm UTC"
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
                }
            });

            WebSiteOptions options = new WebSiteOptions()
            {
                WebSiteTitle = SITE_NAME
            };

            controller = new Controllers.HomeController(articleStore, options);
        }

        [Test]
        public void TestIndex()
        {
            var result = controller.Index() as ViewResult;
            Assert.IsInstanceOf<HomePageItems>(result.Model);

            var model = result.Model as HomePageItems;
            Assert.AreSame(articleStore.Articles[1], model.Article1);
            Assert.AreSame(articleStore.Articles[2], model.Article2);
        }

        [Test]
        public void TestAbout()
        {
            var result = controller.About() as ViewResult;
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void TestPrivacy()
        {
            var result = controller.Privacy() as ViewResult;
            Assert.IsInstanceOf<ViewResult>(result);
            string markdownContent = result.ViewData[Constants.VIEWDATA_MARKDOWN].ToString();
            Assert.IsTrue(markdownContent.Contains(SITE_NAME));
        }

    }
}