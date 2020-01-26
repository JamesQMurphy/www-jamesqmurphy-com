using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class DefaultMarkdownHtmlRenderer : IMarkdownHtmlRenderer
    {
        private readonly Markdig.MarkdownPipeline pipelineUnsafe;
        private readonly Markdig.MarkdownPipeline pipelineSafe;

        public DefaultMarkdownHtmlRenderer()
        {
            var pipelineBuilderUnsafe = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(pipelineBuilderUnsafe);
            Markdig.MarkdownExtensions.UseNoFollowLinks(pipelineBuilderUnsafe);
            Markdig.MarkdownExtensions.UseBootstrap(pipelineBuilderUnsafe);
            pipelineUnsafe = pipelineBuilderUnsafe.Build();

            var pipelineBuilderSafe = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.UseNoFollowLinks(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.UseBootstrap(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.DisableHtml(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.DisableHeadings(pipelineBuilderSafe);
            pipelineSafe = pipelineBuilderSafe.Build();
        }

        public string RenderHtml(string markdown)
        {
            if (markdown == null) throw new ArgumentNullException("markdown");

            var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            renderer.ObjectWriteBefore += Renderer_ObjectWriteBefore;
            pipelineUnsafe.Setup(renderer);

            var document = Markdig.Markdown.Parse(markdown, pipelineUnsafe);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        }

        public string RenderHtmlSafe(string markdown)
        {
            if (markdown == null) throw new ArgumentNullException("markdown");

            var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            renderer.ObjectWriteBefore += Renderer_ObjectWriteBefore;
            pipelineSafe.Setup(renderer);

            var document = Markdig.Markdown.Parse(markdown, pipelineSafe);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        }

        private void Renderer_ObjectWriteBefore(Markdig.Renderers.IMarkdownRenderer arg1, Markdig.Syntax.MarkdownObject obj)
        {
            var linkInline = obj as Markdig.Syntax.Inlines.LinkInline;
            if (linkInline != null)
            {
                OnBeforeWriteLinkInline(linkInline);
            }
        }
        protected virtual void OnBeforeWriteLinkInline(Markdig.Syntax.Inlines.LinkInline linkInline)
        {
        }
    }
}
