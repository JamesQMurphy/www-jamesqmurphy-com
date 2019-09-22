using JamesQMurphy.Web.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly Table _table;
        private readonly Options _options;

        public DynamoDbUserStorage(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            _dynamoDbClient = dynamoDbClient;
            _table = Table.LoadTable(dynamoDbClient, settings.DynamoDbTableName);
            _options = settings;
        }

        //TODO: add cancellation tokens to these methods
        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            _ = await _table.PutItemAsync(FromApplicationUser(user));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            _ = await _table.UpdateItemAsync(FromApplicationUser(user));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            _ = await _table.DeleteItemAsync(user.NormalizedEmail);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress)
        {
            var document = await _table.GetItemAsync(normalizedEmailAddress);
            return ToApplicationUser(document);
        }

        public async Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName)
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

            var result = await _dynamoDbClient.QueryAsync(queryRequest);

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

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            var scanRequest = new ScanRequest
            {
                TableName = _options.DynamoDbTableName,
                IndexName = _options.UserNameIndex,
                Select = Select.ALL_PROJECTED_ATTRIBUTES
            };

            var result = await _dynamoDbClient.ScanAsync(scanRequest);

            return result.Items.Select(d => ToApplicationUser(d));
        }

        private static ApplicationUser ToApplicationUser(Document document)
        {
            return ToApplicationUser(document.ToAttributeMap());
        }

        private static ApplicationUser ToApplicationUser(Dictionary<string, AttributeValue> attributeMap)
        {
            return new ApplicationUser()
            {
                NormalizedEmail = attributeMap[NORMALIZED_EMAIL].S,
                Email = attributeMap[EMAIL].S,
                NormalizedUserName = attributeMap[NORMALIZED_USERNAME].S,
                UserName = attributeMap[USERNAME].S,
                EmailConfirmed = attributeMap.ContainsKey(CONFIRMED) ? attributeMap[CONFIRMED]?.BOOL ?? default(bool) : default(bool),
                PasswordHash = attributeMap.ContainsKey(PASSWORD_HASH) ? attributeMap[PASSWORD_HASH]?.S ?? "" : ""
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
                [PASSWORD_HASH] = user.PasswordHash
            };
        }

    }
}
