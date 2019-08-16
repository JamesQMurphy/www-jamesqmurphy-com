using Amazon.DynamoDBv2;
using JamesQMurphy.Blog.Aws;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class DynamoDbArticleStoreFromConfiguration : DynamoDbArticleStore
    {
        public DynamoDbArticleStoreFromConfiguration(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration) :
            base(dynamoDbClient, configuration["ArticleStore:Path"])
        { }
    }
}
