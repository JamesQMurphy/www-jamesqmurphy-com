using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class DefaultMarkdownHtmlRenderer : IMarkdownHtmlRenderer
    {
        private readonly Markdig.MarkdownPipeline pipeline;

        public DefaultMarkdownHtmlRenderer()
        {
            var pipelineBuilder = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(pipelineBuilder);
            pipeline = pipelineBuilder.Build();
        }
        public string RenderHtml(string markdown) => Markdig.Markdown.ToHtml(markdown, pipeline);
    }
}
