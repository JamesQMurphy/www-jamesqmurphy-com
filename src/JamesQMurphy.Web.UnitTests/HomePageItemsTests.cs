using JamesQMurphy.Blog;
using JamesQMurphy.Web.Models;
using NUnit.Framework;
using System;

namespace JamesQMurphy.Web.UnitTests
{
    public class HomePageItemsTests
    {
        private static string NL = Environment.NewLine;
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
            var content = $"{text}{NL}{NL}#Heading";

            var teaser = HomePageItems.GetArticleTeaserMarkdown(articleFromText(content));
            Assert.AreEqual($"{text}{NL}{NL}", teaser);
        }

        [Test]
        public void TestBreakAtImage()
        {
            var text = $"abcdef";
            var content = $"{text}{NL}{NL}This is some text with an ![image](x.jpg) inside of it.{NL}";

            var teaser = HomePageItems.GetArticleTeaserMarkdown(articleFromText(content));
            Assert.AreEqual($"{text}{NL}{NL}", teaser);
        }

        [Test]
        public void TestBreakAfterPassing500Characters()
        {
            var text = "This is about one hundred characters.  Well I lied; it's only seventy. ";

            var content = $"{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}";

            var teaser = HomePageItems.GetArticleTeaserMarkdown(articleFromText(content));
            Assert.AreEqual($"{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}{NL}{text}{NL}", teaser);
        }
    }
}
