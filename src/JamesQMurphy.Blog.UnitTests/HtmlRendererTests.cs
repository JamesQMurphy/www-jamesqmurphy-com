using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;

namespace Tests
{
    public class HtmlRendererTests
    {
        private IMarkdownHtmlRenderer renderer;

        [SetUp]
        public void Setup()
        {
            renderer = new DefaultMarkdownHtmlRenderer();
        }

        private static void AssertEquivalentHtml(string expected, string actual)
        {
            var expectedHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            expectedHtmlDoc.LoadHtml($"<html><body>{expected}</body></html>");
            var expectedHtml = expectedHtmlDoc.DocumentNode.ChildNodes[0].InnerHtml.Replace("\n", "").Replace("\r","");

            var actualHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            actualHtmlDoc.LoadHtml($"<html><body>{actual}</body></html>");
            var actualHtml = actualHtmlDoc.DocumentNode.ChildNodes[0].InnerHtml.Replace("\n", "").Replace("\r", "");

            Assert.AreEqual(expectedHtml, actualHtml);
        }


        [Test]
        public void TextIsParagraph()
        {
            var text = "some text";
            AssertEquivalentHtml($"<p>{text}</p>", renderer.RenderHtml(text));
        }

        [Test]
        public void CodeFence()
        {
            var text = "Some code;";
            var fenced = $"```\n{text}\n```";

            AssertEquivalentHtml($"<pre><code>{text}\n</code></pre>", renderer.RenderHtml(fenced));
        }

        [Test]
        public void CodeFenceWithLanguage()
        {
            var text = "Some code;";
            var language = "DwimScript";
            var fenced = $"```{language}\n{text}\n```";

            AssertEquivalentHtml($"<pre><code class=\"language-{language}\">{text}\n</code></pre>", renderer.RenderHtml(fenced));
        }

        [Test]
        public void IndentedCode()
        {
            var codeline1 = "Some code;";
            var codeline2 = "Some mode code;";
            var fenced = $"    {codeline1}\n    {codeline2}";

            AssertEquivalentHtml($"<pre><code>{codeline1}\n{codeline2}\n</code></pre>", renderer.RenderHtml(fenced));
        }

        [Test]
        public void Images()
        {
            var altText = "This is alt text";
            var imgSrc = "/a/b/image.png";
            var markdown = $"![{altText}]({imgSrc})";

            AssertEquivalentHtml($"<p><img src=\"{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", renderer.RenderHtml(markdown));
        }

        [Test]
        public void ImagesWithBaseUrl()
        {
            var altText = "This is alt text";
            var imgSrc = "image.png";
            var baseUrl = "/base";
            var markdown = $"![{altText}]({imgSrc})";
            var rendererWithBaseUrl = new DefaultMarkdownHtmlRenderer(baseUrl);

            AssertEquivalentHtml($"<p><img src=\"{baseUrl}/{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", rendererWithBaseUrl.RenderHtml(markdown));
        }

        [Test]
        public void ImagesWithBaseUrl2()
        {
            var altText = "This is alt text";
            var imgSrc = "image.png";
            var baseUrl = "/base/";
            var markdown = $"![{altText}]({imgSrc})";
            var rendererWithBaseUrl = new DefaultMarkdownHtmlRenderer(baseUrl);

            AssertEquivalentHtml($"<p><img src=\"{baseUrl}{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", rendererWithBaseUrl.RenderHtml(markdown));
        }

        [Test]
        public void Footnotes()
        {
            var markdown = "Here[^1] is a footnote.\n\n[^1]: Footnote\n\nOther text.";
            var expected = @"<p>Here<a id=""fnref:1"" href=""#fn:1"" class=""footnote-ref""><sup>1</sup></a> is a footnote.</p><p>Other text.</p><div class=""footnotes""><hr /><ol><li id=""fn:1""><p>Footnote<a href = ""#fnref:1"" class=""footnote-back-ref"">&#8617;</a></p></li></ol></div>";
            var actual = renderer.RenderHtml(markdown);

            AssertEquivalentHtml(expected, actual);
        }


    }
}
