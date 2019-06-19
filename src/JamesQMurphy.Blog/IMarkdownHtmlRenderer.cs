using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public interface IMarkdownHtmlRenderer
    {
        string RenderHtml(string markdown);
    }
}
