using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class DefaultMarkdownHtmlRenderer : IMarkdownHtmlRenderer
    {
        public class Options
        {
            public string LinkBasePath { get; set; }
            public string BlogImageBasePath { get; set; }
        }

        private readonly Options _options;
        private readonly Markdig.MarkdownPipeline pipeline;

        public DefaultMarkdownHtmlRenderer(Options options)
        {
            _options = options;

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
            var link = obj as Markdig.Syntax.Inlines.LinkInline;
            if (link != null && link.IsImage && !(link.Url.StartsWith("/")))
            {
                link.Url = $"{imageBasePath}{link.Url}";
            }
        }

        private static string _combine(string link, string part)
        {
            if (String.IsNullOrWhiteSpace(link))
            {
                return part;
            }
            if (link.EndsWith("/") && part.StartsWith("/"))
            {
                return link + part.Substring(1);
            }
            if (link.EndsWith("/") || part.StartsWith("/"))
            {
                return link + part;
            }
            return link + "/" + part;
        }

    }
}
