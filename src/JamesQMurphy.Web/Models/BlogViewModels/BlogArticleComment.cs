using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.BlogViewModels
{
    public class BlogArticleComment
    {
        public string ArticleSlug { get; set; }
        public string AuthorName { get; set; }
        public string Timestamp { get; set; }
        public bool IsMine { get; set; }
        public string HtmlContent { get; set; }
    }
}
