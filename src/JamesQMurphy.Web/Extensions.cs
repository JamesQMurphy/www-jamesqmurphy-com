using System;
using JamesQMurphy.Blog;
using JamesQMurphy.Blog.Aws;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensionMethods
    {
        // Thank you Filip W. from StrathWeb for this idea!
        // https://www.strathweb.com/2016/09/strongly-typed-configuration-in-asp-net-core-without-ioptionst/
        public static TConfig ConfigurePoco<TConfig>(this IServiceCollection services, IConfiguration configuration, string configSection = "") where TConfig : class, new()
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new TConfig();
            if (String.IsNullOrWhiteSpace(configSection))
            {
                configuration.Bind(config);
            }
            else
            {
                configuration.GetSection(configSection).Bind(config);
            }
            services.AddSingleton(config);
            return config;
        }

        public static IServiceCollection AddArticleStoreServices(this IServiceCollection collection, IConfiguration configuration)
        {
            switch (configuration["ArticleStore:Service"])
            {
                case "LocalFolder":
                    collection.AddSingleton<IArticleStore>(new LocalFolderArticleStore(configuration["ArticleStore:Path"]));
                    break;

                case "DynamoDb":
                    collection.ConfigurePoco<DynamoDbArticleStore.Options>(configuration, "ArticleStore");
                    collection.AddSingleton<IArticleStore, DynamoDbArticleStore>();
                    break;

                default: //InMemory
                    collection.AddSingleton<IArticleStore, InMemoryArticleStore>();
                    break;
            }
            return collection;
        }
    }
}
