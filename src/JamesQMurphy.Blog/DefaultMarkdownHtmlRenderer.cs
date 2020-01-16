using System;
using System.Collections.Generic;
using System.IO;
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
            Markdig.MarkdownExtensions.UseBootstrap(pipelineBuilder);
            pipeline = pipelineBuilder.Build();
        }

        public string RenderHtml(string markdown)
        {
            if (markdown == null) throw new ArgumentNullException("markdown");

            var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            renderer.ObjectWriteBefore += Renderer_ObjectWriteBefore;
            pipeline.Setup(renderer);

            var document = Markdig.Markdown.Parse(markdown, pipeline);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        }

        private void Renderer_ObjectWriteBefore(Markdig.Renderers.IMarkdownRenderer arg1, Markdig.Syntax.MarkdownObject obj)
        {
            var linkInline = obj as Markdig.Syntax.Inlines.LinkInline;
            if (linkInline != null )
            {
                OnBeforeWriteLinkInline(linkInline);
            }
        }

        protected virtual void OnBeforeWriteLinkInline(Markdig.Syntax.Inlines.LinkInline linkInline)
        {
        }

    }
}
