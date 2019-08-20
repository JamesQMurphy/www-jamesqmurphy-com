using JamesQMurphy.Blog;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.ViewComponents
{
    public class ArticleMetadataListing : ViewComponent
    {
        public IViewComponentResult Invoke(ArticleMetadata articleMetadata, bool displayCompact)
        {
            if (displayCompact)
            {
                return View("compact", articleMetadata);
            }
            else
            {
                return View(articleMetadata);
            }
        }
    }
}
