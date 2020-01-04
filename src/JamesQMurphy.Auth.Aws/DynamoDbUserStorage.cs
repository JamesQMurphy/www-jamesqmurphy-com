using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JamesQMurphy.Auth.Aws
{
    public class DynamoDbUserStorage : IApplicationUserStorage
    {
        public class Options
        {
            public string DynamoDbTableName { get; set; }
            public string ProviderIndex { get; set; }
        }

        private const string USER_ID = "userId";
        private const string LAST_UPDATED = "lastUpdated";
        private const string PROVIDER = "provider";
        private const string NORMALIZED_KEY = "normalizedKey";
        private const string KEY = "key";

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _table;
        private readonly Options _options;

        public DynamoDbUserStorage(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dynamoDbClient = dynamoDbClient;
            _table = Table.LoadTable(dynamoDbClient, settings.DynamoDbTableName);
            _options = settings;
        }

        public async Task<ApplicationUserRecord> SaveAsync(ApplicationUserRecord applicationUserRecord, CancellationToken cancellationToken = default)
        {
            var document = FromApplicationUserRecord(applicationUserRecord);
            var documentReturned = await _table.PutItemAsync(document, cancellationToken);
            return ToApplicationUserRecord(documentReturned.ToAttributeMap());
        }

        public async Task<ApplicationUserRecord> DeleteAsync(ApplicationUserRecord applicationUserRecord, CancellationToken cancellationToken = default)
        {
            var document = await _table.DeleteItemAsync(FromApplicationUserRecord(applicationUserRecord), cancellationToken);
            return ToApplicationUserRecord(document.ToAttributeMap());
        }

        public async Task<IEnumerable<ApplicationUserRecord>> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{USER_ID} = :v_userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_userId", new AttributeValue {S = userId} }
                },
                ScanIndexForward = true
            };

            var result = await _dynamoDbClient.QueryAsync(queryRequest, cancellationToken);
            return result.Items.Select(item => ToApplicationUserRecord(item));
        }

        public async Task<IEnumerable<ApplicationUserRecord>> FindByEmailAddressAsync(string normalizedEmailAddress, CancellationToken cancellationToken)
        {
            return await _findByProviderAndKeyAsync(ApplicationUserRecord.RECORD_TYPE_EMAIL, normalizedEmailAddress, cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUserRecord>> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _findByProviderAndKeyAsync(ApplicationUserRecord.RECORD_TYPE_USERNAME, normalizedUserName, cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUserRecord>> GetAllUserRecordsAsync(CancellationToken cancellationToken = default)
        {
            var scanRequest = new ScanRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES
            };

            var result = await _dynamoDbClient.ScanAsync(scanRequest, cancellationToken);
            return result.Items.Select(item => ToApplicationUserRecord(item));
        }

        private async Task<IEnumerable<ApplicationUserRecord>> _findByProviderAndKeyAsync(string provider, string providerKey, CancellationToken cancellationToken)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.ProviderIndex,
                Select = Select.ALL_PROJECTED_ATTRIBUTES,
                KeyConditionExpression = $"{PROVIDER} = :v_provider and {NORMALIZED_KEY} = :v_normalizedKey",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_provider", new AttributeValue {S = provider} },
                    {":v_normalizedKey", new AttributeValue {S = providerKey} }
                },
                ScanIndexForward = true
            };

            var result = await _dynamoDbClient.QueryAsync(queryRequest, cancellationToken);

            if (result.Count == 0)
            {
                return Enumerable.Empty<ApplicationUserRecord>();
            }
            if (result.Count == 1)
            {
                return await FindByIdAsync(result.Items[0][USER_ID].S, cancellationToken);
            }
            throw new Exception($"Found {result.Count} items with provider={provider} and providerKey={providerKey}");

        }

        private static ApplicationUserRecord ToApplicationUserRecord(Dictionary<string, AttributeValue> attributeMap)
        {
            string provider = string.Empty;
            string key = string.Empty;
            string normalizedKey = string.Empty;
            string userId = string.Empty;
            Dictionary<string, string> stringAttributes = new Dictionary<string, string>();
            Dictionary<string, bool> boolAttributes = new Dictionary<string, bool>();
            DateTime lastUpdated = DateTime.MinValue;

            foreach (var attr in attributeMap.Keys)
            {
                switch(attr)
                {
                    case PROVIDER:
                        provider = attributeMap[attr].S;
                        break;

                    case KEY:
                        key = attributeMap[attr].S;
                        break;

                    case NORMALIZED_KEY:
                        normalizedKey = attributeMap[attr].S;
                        break;

                    case USER_ID:
                        userId = attributeMap[attr].S;
                        break;

                    case LAST_UPDATED:
                        DateTime.TryParse(attributeMap[attr].S, out lastUpdated);
                        break;

                    default:
                        if (attributeMap[attr].IsBOOLSet)
                        {
                            boolAttributes[attr] = attributeMap[attr].BOOL;
                        }
                        else if (attributeMap[attr].S != null)
                        {
                            stringAttributes[attr] = attributeMap[attr].S;
                        }
                        break;
                }
            }

            var record = new ApplicationUserRecord(
                    provider,
                    key,
                    userId,
                    normalizedKey,
                    lastUpdated,
                    stringAttributes,
                    boolAttributes
                );

            return record;
        }

        private static Document FromApplicationUserRecord(ApplicationUserRecord userRecord)
        {
            var document = new Document
            {
                [PROVIDER] = userRecord.Provider,
                [KEY] = userRecord.Key,
                [NORMALIZED_KEY] = userRecord.NormalizedKey,
                [USER_ID] = userRecord.UserId,
                [LAST_UPDATED] = DateTime.UtcNow.ToString("O")
            };
            foreach (var attr in userRecord.StringAttributes.Keys)
            {
                document[attr] = userRecord.StringAttributes[attr];
            }
            foreach (var attr in userRecord.BoolAttributes.Keys)
            {
                document[attr] = new DynamoDBBool(userRecord.BoolAttributes[attr]);
            }
            return document;
        }

    }
}
