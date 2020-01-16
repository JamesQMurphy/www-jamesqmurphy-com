using JamesQMurphy.Blog;
using JamesQMurphy.Web.Extensions;
using JamesQMurphy.Web.Models;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Http;
using System;

namespace JamesQMurphy.Web.Services
{
    public class WebsiteMarkupRenderer : DefaultMarkdownHtmlRenderer
    {
        private WebSiteOptions _webSiteOptions;
        private IHttpContextAccessor _httpContextAccessor;

        public WebsiteMarkupRenderer(WebSiteOptions webSiteOptions, IHttpContextAccessor httpContextAccessor)
        {
            _webSiteOptions = webSiteOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnBeforeWriteLinkInline(LinkInline linkInline)
        {
            if (!linkInline.Url.StartsWith("http"))
            {
                var baseUri = new Uri(_webSiteOptions.GetSiteUrlFallbackToContext(_httpContextAccessor));
                if (linkInline.IsImage && (!linkInline.Url.Contains('/')))
                {
                    linkInline.Url = new Uri(baseUri, new Uri(new Uri(_webSiteOptions.ImageBasePath), linkInline.Url)).ToString();
                }
                else
                {
                    linkInline.Url = new Uri(baseUri, linkInline.Url).ToString();
                }
            }
        }

    }
}
