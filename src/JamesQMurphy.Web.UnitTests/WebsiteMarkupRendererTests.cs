using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;
using NUnit.Framework;

namespace JamesQMurphy.Web.UnitTests
{
    public class WebsiteMarkupRendererTests
    {
        [Test]
        public void ImagesWithBaseUrl()
        {
            var altText = "This is alt text";
            var imgSrc = "image.png";
            var imageBase = "/base";
            var markdown = $"![{altText}]({imgSrc})";

            var webSiteOptions = new WebSiteOptions
            {
                ImageBasePath = imageBase
            };
            var renderer = new WebsiteMarkupRenderer(webSiteOptions, null);
            AssertEquivalentHtml($"<p><img src=\"{imageBase}/{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", renderer.RenderHtml(markdown));
        }

        [Test]
        public void LinksWithBaseUrl()
        {
            var linkhref = "page1.htm";
            var baseUrl = "http://localhost";
            var text = "Some link";
            var markdown = $"[{text}]({linkhref})";

            var webSiteOptions = new WebSiteOptions
            {
                ImageBasePath = "somethingElse",
                SiteUrl = baseUrl
            };
            var renderer = new WebsiteMarkupRenderer(webSiteOptions, null);
            AssertEquivalentHtml($"<p><a href=\"{baseUrl}/{linkhref}\" rel=\"nofollow\">{text}</a></p>", renderer.RenderHtml(markdown));
        }

        [Test]
        public void LinksWithNoBaseUrl()
        {
            var linkhref = "page1.htm";
            var baseUrl = "";
            var text = "Some link";
            var markdown = $"[{text}]({linkhref})";

            var webSiteOptions = new WebSiteOptions
            {
                ImageBasePath = "somethingElse",
                SiteUrl = baseUrl
            };
            var renderer = new WebsiteMarkupRenderer(webSiteOptions, null);
            AssertEquivalentHtml($"<p><a href=\"{linkhref}\" rel=\"nofollow\">{text}</a></p>", renderer.RenderHtml(markdown));
        }

        [Test]
        public void DeepLinksWithBaseUrl()
        {
            var linkhref = "abc/page1.htm";
            var baseUrl = "http://localhost";
            var text = "Some link";
            var markdown = $"[{text}]({linkhref})";

            var webSiteOptions = new WebSiteOptions
            {
                ImageBasePath = "somethingElse",
                SiteUrl = baseUrl
            };
            var renderer = new WebsiteMarkupRenderer(webSiteOptions, null);
            AssertEquivalentHtml($"<p><a href=\"{baseUrl}/{linkhref}\" rel=\"nofollow\">{text}</a></p>", renderer.RenderHtml(markdown));
        }

        [Test]
        public void ExternalLinkWithBaseUrl()
        {
            var linkhref = "http://127.0.0.1/abc/page1.htm";
            var baseUrl = "http://localhost";
            var text = "Some link";
            var markdown = $"[{text}]({linkhref})";

            var webSiteOptions = new WebSiteOptions
            {
                ImageBasePath = "somethingElse",
                SiteUrl = baseUrl
            };
            var renderer = new WebsiteMarkupRenderer(webSiteOptions, null);
            AssertEquivalentHtml($"<p><a href=\"{linkhref}\" rel=\"nofollow\">{text}</a></p>", renderer.RenderHtml(markdown));
        }

        private static void AssertEquivalentHtml(string expected, string actual)
        {
            var expectedHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            expectedHtmlDoc.LoadHtml($"<html><body>{expected}</body></html>");
            var expectedHtml = expectedHtmlDoc.DocumentNode.ChildNodes[0].InnerHtml.Replace("\n", "").Replace("\r", "");

            var actualHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            actualHtmlDoc.LoadHtml($"<html><body>{actual}</body></html>");
            var actualHtml = actualHtmlDoc.DocumentNode.ChildNodes[0].InnerHtml.Replace("\n", "").Replace("\r", "");

            Assert.AreEqual(expectedHtml, actualHtml);
        }


    }
}
