using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models
{
    public class ArticleStoreOptions
    {
        public ArticleStoreService Service { get; set; } = ArticleStoreService.InMemory;
        public string Path { get; set; } = "/";
    }

    public enum ArticleStoreService
    {
        InMemory = 0,
        LocalFolder,
        DynamoDb
    }

}

namespace Microsoft.Extensions.Configuration
{
    public static class ArticleStoreExtensionMethods
    {
        public static JamesQMurphy.Web.Models.ArticleStoreOptions GetArticleStoreOptions(this IConfiguration config, string configSection)
        {
            var options = new JamesQMurphy.Web.Models.ArticleStoreOptions();
            config.GetSection(configSection).Bind(options);
            return options;
        }
    }
}