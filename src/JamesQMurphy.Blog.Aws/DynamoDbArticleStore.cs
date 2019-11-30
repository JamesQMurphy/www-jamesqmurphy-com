using System;
using System.Collections.Generic;
using JamesQMurphy.Blog;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;

namespace JamesQMurphy.Blog.Aws
{
    public class DynamoDbArticleStore : IArticleStore
    {
        public class Options
        {
            public string DynamoDbTableName { get; set; }
            public string DynamoDbIndexName { get; set; }
        }

        private const string SLUG = "slug";
        private const string TIMESTAMP = "timestamp";
        private const string TITLE = "title";
        private const string DESCRIPTION = "description";
        //private const string PUBLISH_DATE = "publishDate";
        private const string CONTENT = "content";
        private const string ARTICLE_TYPE = "articleType";
        private const string ARTICLE_TYPE_PUBLISHED = "published";

        private readonly IAmazonDynamoDB _dbClient;
        private readonly Options _options;

        public DynamoDbArticleStore(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dbClient = dynamoDbClient;
            _options = settings;
        }

        public Article GetArticle(string slug)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{SLUG} = :v_slug",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_articleType", new AttributeValue { S = slug }}
                },
                ScanIndexForward = true
            };

            var result = _dbClient.QueryAsync(queryRequest).GetAwaiter().GetResult();
            if (result.Items.Count > 0)
            {
                var item = result.Items[0];
                return new Article()
                {
                    Metadata = ToArticleMetadata(item),
                    Content = item[CONTENT].S
                };
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ArticleMetadata> GetArticles(DateTime startDate, DateTime endDate)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.DynamoDbIndexName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{ARTICLE_TYPE} = :v_articleType and #ts BETWEEN :v_startDate and :v_endDate",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#ts", TIMESTAMP }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_articleType", new AttributeValue { S = ARTICLE_TYPE_PUBLISHED }},
                    {":v_startDate", new AttributeValue { S = startDate.ToString("O") }},
                    {":v_endDate", new AttributeValue { S = endDate.ToString("O") }}
                },
                ScanIndexForward = false,
            };
            var result = _dbClient.QueryAsync(queryRequest).GetAwaiter().GetResult();
            return result.Items.ConvertAll(i => ToArticleMetadata(i));
        }

        public IEnumerable<ArticleMetadata> GetLastArticles(int numberOfArticles)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.DynamoDbIndexName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{ARTICLE_TYPE} = :v_articleType and #ts < :v_now",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#ts", TIMESTAMP }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_articleType", new AttributeValue { S = ARTICLE_TYPE_PUBLISHED }},
                    {":v_now", new AttributeValue { S = DateTime.UtcNow.ToString("O") }}
                },
                Limit = numberOfArticles,
                ScanIndexForward = false,
            };
            var result = _dbClient.QueryAsync(queryRequest).GetAwaiter().GetResult();
            return result.Items.ConvertAll(i => ToArticleMetadata(i));
        }


        private static ArticleMetadata ToArticleMetadata(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ArticleMetadata()
            {
                Slug = attributeMap[SLUG].S,
                PublishDate = DateTime.Parse(attributeMap[TIMESTAMP].S).ToUniversalTime(),
                Title = attributeMap[TITLE].S,
                Description = attributeMap.ContainsKey(DESCRIPTION) ? attributeMap[DESCRIPTION].S : ""
            };
        }

        private static Document FromArticleMetadata(ArticleMetadata user)
        {
            var d = new Document
            {
                [SLUG] = user.Slug,
                [TIMESTAMP] = user.PublishDate.ToString("O"),
                [TITLE] = user.Title
            };
            if (!String.IsNullOrWhiteSpace(user.Description))
            {
                d[DESCRIPTION] = user.Description;
            }
            return d;
        }

    }
}
