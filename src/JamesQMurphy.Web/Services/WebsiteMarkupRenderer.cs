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
                if (linkInline.IsImage && (!linkInline.Url.Contains('/')) && (!String.IsNullOrWhiteSpace(_webSiteOptions.ImageBasePath)))
                {
                    if (_webSiteOptions.ImageBasePath.EndsWith('/'))
                    {
                        linkInline.Url = $"{_webSiteOptions.ImageBasePath}{linkInline.Url}";
                    }
                    else
                    {
                        linkInline.Url = $"{_webSiteOptions.ImageBasePath}/{linkInline.Url}";
                    }
                }

                var baseUrl = _webSiteOptions.GetSiteUrlFallbackToContext(_httpContextAccessor?.HttpContext);
                if (!String.IsNullOrWhiteSpace(baseUrl))
                {
                    linkInline.Url = new Uri(new Uri(baseUrl), linkInline.Url).ToString();
                }
            }
        }

    }
}
