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
        private const string PUBLISH_DATE = "publishDate";
        private const string TITLE = "title";
        private const string DESCRIPTION = "description";
        private const string CONTENT = "content";

        private readonly List<string> metadataAttributesToGet = new List<string> { SLUG, TITLE, PUBLISH_DATE, DESCRIPTION };

        private readonly IAmazonDynamoDB _dbClient;
        private readonly Options _options;

        public DynamoDbArticleStore(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dbClient = dynamoDbClient;
            _options = settings;
        }

        public Article GetArticle(string slug)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.DynamoDbIndexName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{SLUG} = :v_slug",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_slug", new AttributeValue { S =  slug }}
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
                KeyConditionExpression = $"{PUBLISH_DATE} >= :v_startDate and {PUBLISH_DATE} <= :v_endDate",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_startDate", new AttributeValue { S =  startDate.ToString("O") }},
                    {":v_endDate", new AttributeValue { S =  endDate.ToString("O") }}
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
                ScanIndexForward = false
            };
            var result = _dbClient.QueryAsync(queryRequest).GetAwaiter().GetResult();
            return result.Items.ConvertAll(i => ToArticleMetadata(i));
        }


        private static ArticleMetadata ToArticleMetadata(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ArticleMetadata()
            {
                Slug = attributeMap[SLUG].S,
                PublishDate = DateTime.Parse(attributeMap[PUBLISH_DATE].S).ToUniversalTime(),
                Title = attributeMap[TITLE].S,
                Description = attributeMap.ContainsKey(DESCRIPTION) ? attributeMap[DESCRIPTION].S : ""
            };
        }

        private static Document FromArticleMetadata(ArticleMetadata user)
        {
            var d = new Document
            {
                [SLUG] = user.Slug,
                [PUBLISH_DATE] = user.PublishDate.ToString("O"),
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
