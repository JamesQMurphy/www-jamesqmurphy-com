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
        private readonly string imageBasePath;

        public DefaultMarkdownHtmlRenderer(string ImageBasePath = "")
        {
            var pipelineBuilderUnsafe = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(pipelineBuilderUnsafe);
            Markdig.MarkdownExtensions.UseBootstrap(pipelineBuilderUnsafe);
            pipelineUnsafe = pipelineBuilderUnsafe.Build();

            var pipelineBuilderSafe = new Markdig.MarkdownPipelineBuilder();
            Markdig.MarkdownExtensions.UseAdvancedExtensions(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.UseBootstrap(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.DisableHtml(pipelineBuilderSafe);
            Markdig.MarkdownExtensions.DisableHeadings(pipelineBuilderSafe);
            pipelineSafe = pipelineBuilderSafe.Build();

            if (ImageBasePath == null)
            {
                imageBasePath = "";
            }
            else
            {
                if (ImageBasePath.EndsWith("/"))
                {
                    imageBasePath = ImageBasePath;
                }
                else
                {
                    imageBasePath = ImageBasePath + "/";
                }
            }
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
            pipelineSafe.Setup(renderer);

            var document = Markdig.Markdown.Parse(markdown, pipelineSafe);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        }

        private void Renderer_ObjectWriteBefore(Markdig.Renderers.IMarkdownRenderer arg1, Markdig.Syntax.MarkdownObject obj)
        {
            var link = obj as Markdig.Syntax.Inlines.LinkInline;
            if (link != null && link.IsImage && !(link.Url.StartsWith("/")))
            {
                link.Url = $"{imageBasePath}{link.Url}";
            }
        }
    }
}
