﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Blog
{
    public interface IArticleStore
    {
        Task<Article> GetArticleAsync(string slug);
        Task<IEnumerable<ArticleMetadata>> GetArticlesAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ArticleMetadata>> GetLastArticlesAsync(int numberOfArticles);
        Task<IEnumerable<ArticleComment>> GetArticleComments(string articleSlug, string sinceArticleId = "", int pageSize = 50, bool latest = false);
        Task<bool> AddComment(string articleSlug, string content, string userId, string userName, DateTime timestamp, string replyingTo = "");
    }
}
