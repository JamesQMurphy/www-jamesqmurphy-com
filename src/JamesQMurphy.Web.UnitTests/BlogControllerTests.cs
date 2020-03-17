using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;

namespace JamesQMurphy.Web.UnitTests
{
    public class BlogControllerTests
    {
        private InMemoryArticleStore articleStore;
        private Controllers.blogController controller;

        [SetUp]
        public void Setup()
        {
            articleStore = new InMemoryArticleStore();
            controller = new Controllers.blogController(articleStore, new DefaultMarkdownHtmlRenderer(), null, new WebSiteOptions());
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                // No claims; i.e., not logged in
                User = new System.Security.Claims.ClaimsPrincipal()
            };
        }

        [Test]
        public void TestDetailNoDescription()
        {
            int year = 2019;
            int month = 10;
            string slug = "slug";
            string title = "Title for Test Detail";

            var article = new Article()
            {
                Title = title,
                PublishDate = new DateTime(year, month, 1),
                Slug = $"{year}/{month}/{slug}"
            };
            articleStore.SafeAddArticle(article);

            var result = controller.details(year.ToString(), month.ToString(), slug).GetAwaiter().GetResult() as ViewResult;
            Assert.AreSame(article, result.Model);
            Assert.AreEqual(title, result.ViewData[Constants.VIEWDATA_PAGETITLE]);
        }

        [Test]
        public void TestDetailWithDescription()
        {
            int year = 2019;
            int month = 10;
            string slug = "slug";
            string title = "Title for Test Detail";
            string description = "Some description for the ages";

            var article = new Article()
            {
                Title = title,
                PublishDate = new DateTime(year, month, 1),
                Slug = $"{year}/{month}/{slug}",
                Description = description
            };
            articleStore.SafeAddArticle(article);

            var result = controller.details(year.ToString(), month.ToString(), slug).GetAwaiter().GetResult() as ViewResult;
            Assert.AreSame(article, result.Model);
            Assert.AreEqual($"{title}: {description}", result.ViewData[Constants.VIEWDATA_PAGETITLE]);
        }

    }
}