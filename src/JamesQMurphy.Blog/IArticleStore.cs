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
    }
}
