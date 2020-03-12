using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public interface IMarkdownHtmlRenderer
    {
        string RenderHtml(string markdown);
        string RenderHtmlSafe(string markdown, bool keepLineBreaks = false);
    }
}
