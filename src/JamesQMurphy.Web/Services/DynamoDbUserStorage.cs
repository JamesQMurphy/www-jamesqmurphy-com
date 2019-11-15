using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class DynamoDbUserStorage : IApplicationUserStorage
    {
        public class Options
        {
            public string DynamoDbTableName { get; set; }
            public string UserNameIndex { get; set; }
        }

        private const string NORMALIZED_EMAIL = "normalizedEmail";
        private const string EMAIL = "email";
        private const string NORMALIZED_USERNAME = "normalizedUserName";
        private const string USERNAME = "userName";
        private const string CONFIRMED = "confirmed";
        private const string PASSWORD_HASH = "passwordHash";
        private const string LAST_UPDATED = "lastUpdated";
        private const string IS_ADMINISTRATOR = "isAdministrator";

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _table;
        private readonly Options _options;

        public DynamoDbUserStorage(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dynamoDbClient = dynamoDbClient;
            _table = Table.LoadTable(dynamoDbClient, settings.DynamoDbTableName);
            _options = settings;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.LastUpdated = DateTime.UtcNow;
            _ = await _table.PutItemAsync(FromApplicationUser(user), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.LastUpdated = DateTime.UtcNow;
            _ = await _table.UpdateItemAsync(FromApplicationUser(user), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            _ = await _table.DeleteItemAsync(user.NormalizedEmail, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = $"{NORMALIZED_EMAIL} = :v_normalizedEmail",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_normalizedEmail", new AttributeValue {S = normalizedEmailAddress} }
                },
                ScanIndexForward = true
            };

            var result = await _dynamoDbClient.QueryAsync(queryRequest, cancellationToken);

            if (result.Count == 0)
            {
                return null;
            }
            if (result.Count == 1)
            {
                return ToApplicationUser(result.Items[0]);
            }
            throw new Exception($"Found {result.Count} items with normalizedEmailAddress = {normalizedEmailAddress}");
        }

        public async Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryRequest = new QueryRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.UserNameIndex,
                Select = Select.ALL_PROJECTED_ATTRIBUTES,
                KeyConditionExpression = $"{NORMALIZED_USERNAME} = :v_normalizedUserName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_normalizedUserName", new AttributeValue {S = normalizedUserName} }
                },
                ScanIndexForward = true
            };

            var result = await _dynamoDbClient.QueryAsync(queryRequest, cancellationToken);

            if (result.Count == 0)
            {
                return null;
            }
            if (result.Count == 1)
            {
                return ToApplicationUser(result.Items[0]);
            }
            throw new Exception($"Found {result.Count} items with normalizedUserName = {normalizedUserName}");
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var scanRequest = new ScanRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.UserNameIndex,
                Select = Select.ALL_PROJECTED_ATTRIBUTES
            };

            var result = await _dynamoDbClient.ScanAsync(scanRequest, cancellationToken);

            return result.Items.Select(d => ToApplicationUser(d));
        }

        private static ApplicationUser ToApplicationUser(Dictionary<string, AttributeValue> attributeMap)
        {
            DateTime.TryParse(attributeMap.ContainsKey(LAST_UPDATED) ? attributeMap[LAST_UPDATED].S : "", out DateTime lastUpdated);

            return new ApplicationUser()
            {
                NormalizedEmail = attributeMap[NORMALIZED_EMAIL].S,
                Email = attributeMap[EMAIL].S,
                NormalizedUserName = attributeMap[NORMALIZED_USERNAME].S,
                UserName = attributeMap[USERNAME].S,
                EmailConfirmed = attributeMap.ContainsKey(CONFIRMED) ? attributeMap[CONFIRMED]?.BOOL ?? default(bool) : default(bool),
                PasswordHash = attributeMap.ContainsKey(PASSWORD_HASH) ? attributeMap[PASSWORD_HASH]?.S ?? "" : "",
                LastUpdated = lastUpdated,
                IsAdministrator = attributeMap.ContainsKey(IS_ADMINISTRATOR) ? attributeMap[IS_ADMINISTRATOR]?.BOOL ?? false : false
            };
        }

        private static Document FromApplicationUser(ApplicationUser user)
        {
            return new Document
            {
                [NORMALIZED_EMAIL] = user.NormalizedEmail,
                [EMAIL] = user.Email,
                [NORMALIZED_USERNAME] = user.NormalizedUserName,
                [USERNAME] = user.UserName,
                [CONFIRMED] = new DynamoDBBool(user.EmailConfirmed),
                [PASSWORD_HASH] = user.PasswordHash,
                [LAST_UPDATED] = user.LastUpdated.ToString("O"),
                [IS_ADMINISTRATOR] = new DynamoDBBool(user.IsAdministrator)
            };
        }

    }
}
