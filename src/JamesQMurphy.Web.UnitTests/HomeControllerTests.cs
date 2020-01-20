using JamesQMurphy.Auth;
using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace JamesQMurphy.Web.UnitTests
{
    public class HomeControllerTests
    {
        private List<Article> _articles;
        private InMemoryArticleStore _articleStore;
        private Controllers.homeController _controller;
        private const string SITE_NAME = "TEST SITE 820ae666";

        [SetUp]
        public void Setup()
        {
            _articles = new List<Article>();
            _articles.AddRange(new Article[]
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
                    Content = "This is article three, published on July 6, 2019 at 6:34pm UTC"
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
                }
            });

            _articleStore = new InMemoryArticleStore();
            foreach (var article in _articles)
            {
                _articleStore.SafeAddArticle(article);
            }

            WebSiteOptions options = new WebSiteOptions()
            {
                WebSiteTitle = SITE_NAME
            };

            var userManager = ConfigurationHelper
                .CreateServiceProvider()
                .GetService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

            _controller = new Controllers.homeController(_articleStore, options, userManager);
        }

        [Test]
        public void TestIndex()
        {
            var result = _controller.index().GetAwaiter().GetResult() as ViewResult;
            Assert.IsInstanceOf<HomePageItems>(result.Model);

            var model = result.Model as HomePageItems;
            Assert.AreSame(_articles[1], model.Article1);
            Assert.AreSame(_articles[2], model.Article2);
        }

        [Test]
        public void TestAbout()
        {
            var result = _controller.about() as ViewResult;
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void TestPrivacy()
        {
            var result = _controller.privacy() as ViewResult;
            Assert.IsInstanceOf<ViewResult>(result);
            string markdownContent = (result.Model as Article).Content;
            Assert.IsTrue(markdownContent.Contains(SITE_NAME));
        }

    }
}