﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.BlogViewModels
{
    public class BlogArticleReaction
    {
        public string commentId { get; set; }
        public string articleSlug { get; set; }
        public string authorName { get; set; }
        public string authorImageUrl { get; set; }
        public string timestamp { get; set; }
        public bool isMine { get; set; }
        public bool canReply { get; set; }
        public bool canHide { get; set; }
        public bool canDelete { get; set; }
        public string editState { get; set; }
        public string htmlContent { get; set; }
        public string replyToId { get; set; }
    }
}
