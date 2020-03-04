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

        // Columns
        private const string SLUG = "slug";
        private const string TIMESTAMP = "timestamp";
        private const string ARTICLE_TYPE = "articleType";
        private const string TITLE = "title";
        private const string DESCRIPTION = "description";
        private const string CONTENT = "content";
        private const string STATUS = "status";
        private const string AUTHOR_ID = "authorId";
        private const string AUTHOR_NAME = "authorName";

        // Column values
        private const string ARTICLE_TYPE_PUBLISHED = "published";
        private const string ARTICLE_TYPE_COMMENT = "comment";
        private const string ARTICLE_TYPE_COMMENTEDIT = "commentEdit";
        private const string ARTICLE_TYPE_COMMENTHIDE = "commentHide";
        private const string ARTICLE_TYPE_COMMENTDELETE = "commentDelete";
        private const string STATUS_LOCKED = "locked";
        private const string STATUS_ORIGINAL = "original";
        private const string STATUS_EDITED = "edited";
        private const string STATUS_HIDDEN = "hidden";
        private const string STATUS_DELETED = "deleted";

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

        public async Task<string> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
        {
            var reactionId = new ArticleReactionTimestampId(timestamp, replyingTo);
            var d = new Document
            {
                [SLUG] = articleSlug,
                [TIMESTAMP] = reactionId.ToString(),
                [CONTENT] = content,
                [AUTHOR_ID] = userId,
                [AUTHOR_NAME] = userName,
            };
            switch (articleReactionType)
            {
                case ArticleReactionType.Comment:
                    d[ARTICLE_TYPE] = ARTICLE_TYPE_COMMENT;
                    break;

                case ArticleReactionType.Edit:
                    d[ARTICLE_TYPE] = ARTICLE_TYPE_COMMENTEDIT;
                    break;

                case ArticleReactionType.Hide:
                    d[ARTICLE_TYPE] = ARTICLE_TYPE_COMMENTHIDE;
                    break;

                case ArticleReactionType.Delete:
                    d[ARTICLE_TYPE] = ARTICLE_TYPE_COMMENTDELETE;
                    break;

                default:
                    throw new ArgumentException($"Unknown reaction type {articleReactionType}", "articleReactionType");
            }
            var table = Table.LoadTable(_dbClient, _options.DynamoDbTableName);
            _ = await table.PutItemAsync(d);
            return reactionId.ReactionId;
        }

        private static ArticleMetadata ToArticleMetadata(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ArticleMetadata()
            {
                Slug = attributeMap[SLUG].S,
                PublishDate = DateTime.Parse(attributeMap[TIMESTAMP].S).ToUniversalTime(),
                Title = attributeMap[TITLE].S,
                Description = attributeMap.ContainsKey(DESCRIPTION) ? attributeMap[DESCRIPTION].S : "",
                LockedForComments = attributeMap.ContainsKey(STATUS) && attributeMap[STATUS].S == STATUS_LOCKED,
            };
        }

        private static Document FromArticleMetadata(ArticleMetadata articleMetadata)
        {
            var d = new Document
            {
                [SLUG] = articleMetadata.Slug,
                [TIMESTAMP] = articleMetadata.PublishDate.ToString("O"),
                [TITLE] = articleMetadata.Title
            };
            if (!String.IsNullOrWhiteSpace(articleMetadata.Description))
            {
                d[DESCRIPTION] = articleMetadata.Description;
            }
            if (articleMetadata.LockedForComments)
            {
                d[STATUS] = STATUS_LOCKED;
            }
            return d;
        }

        private static ArticleReaction ToArticleReaction(Dictionary<string, AttributeValue> attributeMap)
        {
            var reaction = new ArticleReaction()
            {
                ArticleSlug = attributeMap[SLUG].S,
                TimestampId = attributeMap[TIMESTAMP].S,
                AuthorId = attributeMap[AUTHOR_ID].S,
                AuthorName = attributeMap[AUTHOR_NAME].S,
                ReactionType = (ArticleReactionType)Enum.Parse(typeof(ArticleReactionType), attributeMap[ARTICLE_TYPE].S),
                EditState = attributeMap.ContainsKey(STATUS) ? (ArticleReactionEditState)Enum.Parse(typeof(ArticleReactionEditState), attributeMap[STATUS].S) : ArticleReactionEditState.Original,
                Content = attributeMap[CONTENT].S,
            };
            if (attributeMap.ContainsKey(STATUS))
            {
                switch (attributeMap[STATUS].S)
                {
                    case STATUS_ORIGINAL:
                        reaction.EditState = ArticleReactionEditState.Original;
                        break;

                    case STATUS_EDITED:
                        reaction.EditState = ArticleReactionEditState.Edited;
                        break;

                    case STATUS_HIDDEN:
                        reaction.EditState = ArticleReactionEditState.Hidden;
                        break;

                    case STATUS_DELETED:
                        reaction.EditState = ArticleReactionEditState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            return reaction;
        }

    }
}
