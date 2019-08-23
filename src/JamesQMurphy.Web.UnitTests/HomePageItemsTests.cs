using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using NUnit.Framework;
using System;

namespace JamesQMurphy.Web.UnitTests
{
    public class HomePageItemsTests
    {

        private Article articleFromText(string text)
        {
            return new Article()
            {
                Content = text,
                Slug = "some-slug",
                Title = "Some Title",
                PublishDate = DateTime.UtcNow
            };
        }

        [Test]
        public void TestBreakAtHeading()
        {
            var text = $"abcdef";
            var content = $"{text}{Environment.NewLine}{Environment.NewLine}#Heading";

            var teaser = HomePageItems.GetArticleTeaserMarkdown(articleFromText(content));
            Assert.AreEqual($"{text}{Environment.NewLine}{Environment.NewLine}", teaser);
        }
    }
}
