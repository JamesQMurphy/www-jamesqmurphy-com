using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;
using System.Xml;

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
            XmlDocument expectedXml = new XmlDocument();
            expectedXml.LoadXml(expected);

            XmlDocument actualXml = new XmlDocument();
            actualXml.LoadXml(actual);

            Assert.AreEqual(expectedXml.OuterXml, actualXml.OuterXml);
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

            AssertEquivalentHtml($"<pre><code class='language-{language}'>{text}\n</code></pre>", renderer.RenderHtml(fenced));
        }

        [Test]
        public void IndentedCode()
        {
            var codeline1 = "Some code;";
            var codeline2 = "Some mode code;";
            var fenced = $"    {codeline1}\n    {codeline2}";

            AssertEquivalentHtml($"<pre><code>{codeline1}\n{codeline2}\n</code></pre>", renderer.RenderHtml(fenced));
        }

        [Test, Ignore("Need to figure this out")]
        public void Images()
        {
            var altText = "This is alt text";
            var imgSrc = "/a/b/image.png";
            var markdown = $"![{altText}]({imgSrc})";

            AssertEquivalentHtml($"<p><img src='{imgSrc}' alt='{altText}'/>", renderer.RenderHtml(markdown));
        }


    }
}
