using JamesQMurphy.Blog;
using JamesQMurphy.Blog.Aws;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensionMethods
    {
        public static IServiceCollection AddArticleStoreServices(this IServiceCollection collection, ArticleStoreOptions options)
        {
            switch (options.Service)
            {
                case ArticleStoreService.LocalFolder:
                    collection.AddSingleton<IArticleStore>(new LocalFolderArticleStore(options.Path));
                    break;

                case ArticleStoreService.InMemory:
                    collection.AddSingleton<IArticleStore, InMemoryArticleStore>();
                    break;

                case ArticleStoreService.DynamoDb:
                    collection.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
                    collection.AddSingleton<IArticleStore, DynamoDbArticleStoreFromConfiguration>();
                    break;
            }
            return collection;
        }
    }
}
