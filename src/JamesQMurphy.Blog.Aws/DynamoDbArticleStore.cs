using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private const string CONTENT = "content";
        private const string ARTICLE_TYPE = "articleType";
        private const string ARTICLE_TYPE_PUBLISHED = "published";
        private const string LOCKED_FOR_COMMENTS = "lockedForComments";
        private const string EDIT_STATE = "editState";

        private readonly IAmazonDynamoDB _dbClient;
        private readonly Options _options;

        public DynamoDbArticleStore(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dbClient = dynamoDbClient;
            _options = settings;
        }

        public async Task<Article> GetArticleAsync(string slug)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{SLUG} = :v_slug",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_slug", new AttributeValue { S = slug }}
                },
                ScanIndexForward = true
            };

            var result = await _dbClient.QueryAsync(queryRequest);
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

        public async Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
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
            var result = await _dbClient.QueryAsync(queryRequest);
            return result.Items.ConvertAll(i => ToArticleMetadata(i));
        }

        public async Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
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
            var result = await _dbClient.QueryAsync(queryRequest);
            return result.Items.ConvertAll(i => new Article { Metadata = ToArticleMetadata(i), Content = i[CONTENT].S });
        }

        public async Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{ARTICLE_TYPE} = :v_articleType and #ts > :v_since",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#ts", TIMESTAMP }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_articleSlug", new AttributeValue { S = articleSlug }},
                    {":v_since", new AttributeValue { S = sinceTimestamp }}
                },
                Limit = pageSize,
                ScanIndexForward = !latest,
            };
            var result = await _dbClient.QueryAsync(queryRequest);
            return result.Items
                .Where( rec => rec[ARTICLE_TYPE].S != ARTICLE_TYPE_PUBLISHED )
                .Select(i => ToArticleReaction(i));
        }

        public Task<string> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            throw new NotImplementedException();
        }

        private static ArticleMetadata ToArticleMetadata(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ArticleMetadata()
            {
                Slug = attributeMap[SLUG].S,
                PublishDate = DateTime.Parse(attributeMap[TIMESTAMP].S).ToUniversalTime(),
                Title = attributeMap[TITLE].S,
                Description = attributeMap.ContainsKey(DESCRIPTION) ? attributeMap[DESCRIPTION].S : "",
                LockedForComments = attributeMap.ContainsKey(LOCKED_FOR_COMMENTS) && attributeMap[LOCKED_FOR_COMMENTS].IsBOOLSet ? attributeMap[LOCKED_FOR_COMMENTS].BOOL : false
            };
        }

        private static Document FromArticleMetadata(ArticleMetadata articleMetadata)
        {
            var d = new Document
            {
                [SLUG] = articleMetadata.Slug,
                [TIMESTAMP] = articleMetadata.PublishDate.ToString("O"),
                [TITLE] = articleMetadata.Title,
                [LOCKED_FOR_COMMENTS] = new DynamoDBBool(articleMetadata.LockedForComments)
            };
            if (!String.IsNullOrWhiteSpace(articleMetadata.Description))
            {
                d[DESCRIPTION] = articleMetadata.Description;
            }
            return d;
        }

        private static ArticleReaction ToArticleReaction(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ArticleReaction()
            {
                ArticleSlug = attributeMap[SLUG].S,
                TimestampId = attributeMap[TIMESTAMP].S,
                AuthorId = "", // TODO
                AuthorName = "", // TODO
                ReactionType = (ArticleReactionType)Enum.Parse(typeof(ArticleReactionType), attributeMap[ARTICLE_TYPE].S),
                EditState = attributeMap.ContainsKey(EDIT_STATE) ? (ArticleReactionEditState)Enum.Parse(typeof(ArticleReactionEditState), attributeMap[EDIT_STATE].S) : ArticleReactionEditState.Original,
                Content = attributeMap[CONTENT].S,
            };
        }


    }
}
