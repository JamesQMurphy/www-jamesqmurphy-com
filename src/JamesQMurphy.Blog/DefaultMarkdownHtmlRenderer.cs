using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class DefaultMarkdownHtmlRenderer : IMarkdownHtmlRenderer
    {
        public string RenderHtml(string markdown) => Markdig.Markdown.ToHtml(markdown);
    }
}
