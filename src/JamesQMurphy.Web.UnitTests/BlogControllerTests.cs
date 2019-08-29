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
        private Controllers.BlogController controller;

        [SetUp]
        public void Setup()
        {
            articleStore = new InMemoryArticleStore();
            controller = new Controllers.BlogController(articleStore);
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
                Slug = slug
            };
            articleStore.Articles.Add(article);

            var result = controller.Details(year.ToString(), month.ToString(), slug) as ViewResult;
            Assert.AreSame(article, result.Model);
            Assert.AreEqual(title, result.ViewData["Title"]);
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
                Slug = slug,
                Description = description
            };
            articleStore.Articles.Add(article);

            var result = controller.Details(year.ToString(), month.ToString(), slug) as ViewResult;
            Assert.AreSame(article, result.Model);
            Assert.AreEqual($"{title}: {description}", result.ViewData["Title"]);
        }

    }
}