using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;

namespace Tests
{
    public class HtmlRendererTests
    {
        private class LinkModifyingRenderer : DefaultMarkdownHtmlRenderer
        {
            public string ImageBase;
            public string LinkBase;

            protected override void OnBeforeWriteLinkInline(Markdig.Syntax.Inlines.LinkInline linkInline)
            {
                if (linkInline.IsImage)
                {
                    linkInline.Url = $"{ImageBase}/{linkInline.Url}";
                }
                else
                {
                    linkInline.Url = $"{LinkBase}/{linkInline.Url}";
                }
            }
        }

        private IMarkdownHtmlRenderer _defaultRenderer;
        private LinkModifyingRenderer _linkModifyingRenderer;

        [SetUp]
        public void Setup()
        {
            _defaultRenderer = new DefaultMarkdownHtmlRenderer();
            _linkModifyingRenderer = new LinkModifyingRenderer();
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
            AssertEquivalentHtml($"<p>{text}</p>", _defaultRenderer.RenderHtml(text));
        }

        [Test]
        public void CodeFence()
        {
            var text = "Some code;";
            var fenced = $"```\n{text}\n```";

            AssertEquivalentHtml($"<pre><code>{text}\n</code></pre>", _defaultRenderer.RenderHtml(fenced));
        }

        [Test]
        public void CodeFenceWithLanguage()
        {
            var text = "Some code;";
            var language = "DwimScript";
            var fenced = $"```{language}\n{text}\n```";

            AssertEquivalentHtml($"<pre><code class=\"language-{language}\">{text}\n</code></pre>", _defaultRenderer.RenderHtml(fenced));
        }

        [Test]
        public void IndentedCode()
        {
            var codeline1 = "Some code;";
            var codeline2 = "Some mode code;";
            var fenced = $"    {codeline1}\n    {codeline2}";

            AssertEquivalentHtml($"<pre><code>{codeline1}\n{codeline2}\n</code></pre>", _defaultRenderer.RenderHtml(fenced));
        }

        [Test]
        public void Images()
        {
            var altText = "This is alt text";
            var imgSrc = "/a/b/image.png";
            var markdown = $"![{altText}]({imgSrc})";

            AssertEquivalentHtml($"<p><img src=\"{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", _defaultRenderer.RenderHtml(markdown));
        }

        [Test]
        public void ImagesWithBaseUrl()
        {
            var altText = "This is alt text";
            var imgSrc = "image.png";
            var baseUrl = "/base";
            var markdown = $"![{altText}]({imgSrc})";

            _linkModifyingRenderer.ImageBase = baseUrl;
            _linkModifyingRenderer.LinkBase = "somethingElse";
            AssertEquivalentHtml($"<p><img src=\"{baseUrl}/{imgSrc}\" class=\"img-fluid\" alt=\"{altText}\"/>", _linkModifyingRenderer.RenderHtml(markdown));
        }

        [Test]
        public void LinksWithBaseUrl()
        {
            var linkhref = "page1.htm";
            var baseUrl = "/base2";
            var text = "Some link";
            var markdown = $"[{text}]({linkhref})";

            _linkModifyingRenderer.ImageBase = "somethingElse";
            _linkModifyingRenderer.LinkBase = baseUrl;
            AssertEquivalentHtml($"<p><a href=\"{baseUrl}/{linkhref}\" rel=\"nofollow\">{text}</a></p>", _linkModifyingRenderer.RenderHtml(markdown));
        }

        [Test]
        public void Footnotes()
        {
            var markdown = "Here[^1] is a footnote.\n\n[^1]: Footnote\n\nOther text.";
            var expected = @"<p>Here<a id=""fnref:1"" href=""#fn:1"" class=""footnote-ref""><sup>1</sup></a> is a footnote.</p><p>Other text.</p><div class=""footnotes""><hr /><ol><li id=""fn:1""><p>Footnote<a href = ""#fnref:1"" class=""footnote-back-ref"">&#8617;</a></p></li></ol></div>";
            var actual = _defaultRenderer.RenderHtml(markdown);

            AssertEquivalentHtml(expected, actual);
        }

        [Test]
        public void ScriptTagsUnsafeOnly()
        {
            var markdown = "<script>alert('Pwned');</script>";
            var expectedUnsafe = @"<script>alert('Pwned');</script>";
            var actualUnsafe = _defaultRenderer.RenderHtml(markdown);
            AssertEquivalentHtml(expectedUnsafe, actualUnsafe);

            var expectedSafe = @"<p>&lt;script&gt;alert('Pwned');&lt;/script&gt;</p>";
            var actualSafe = _defaultRenderer.RenderHtmlSafe(markdown);
            AssertEquivalentHtml(expectedSafe, actualSafe);
        }

        [Test]
        public void LineBreaksNotAskedFor()
        {
            var markdown = "Abc\nDef";
            var expected = @"<p>AbcDef</p>";    // AssertEquivalentHtml strips \n and \r
            var actual = _defaultRenderer.RenderHtmlSafe(markdown, false);

            AssertEquivalentHtml(expected, actual);
        }

        [Test]
        public void LineBreaksAskedFor()
        {
            var markdown = "Abc\nDef";
            var expected = @"<p>Abc<br>Def</p>";
            var actual = _defaultRenderer.RenderHtmlSafe(markdown, true);

            AssertEquivalentHtml(expected, actual);
        }

        [Test]
        public void LineBreaksAskedForButNotNeeded()
        {
            var markdown = "Abc\n\nDef";
            var expected = @"<p>Abc</p><p>Def</p>";
            var actual = _defaultRenderer.RenderHtmlSafe(markdown, true);

            AssertEquivalentHtml(expected, actual);
        }

        [Test]
        public void LineBreaksAskedForButNotNeeded2()
        {
            var markdown = "Abc\n";
            var expected = @"<p>Abc</p>";
            var actual = _defaultRenderer.RenderHtmlSafe(markdown, true);

            AssertEquivalentHtml(expected, actual);
        }


    }
}
