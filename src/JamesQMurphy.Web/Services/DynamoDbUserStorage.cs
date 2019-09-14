using System;
using System.Collections.Generic;
using JamesQMurphy.Web.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class DynamoDbUserStorage : IApplicationUserStorage
    {
        public class Options
        {
            public string DynamoDbTableName { get; set; }
        }

        private readonly Table table;

        public DynamoDbUserStorage(IAmazonDynamoDB dynamoDbClient, Options settings)
        {
            table = Table.LoadTable(dynamoDbClient, settings.DynamoDbTableName);
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser> FindByEmailAddress(string normalizedEmailAddress)
        {
            var document = await table.GetItemAsync(normalizedEmailAddress);
            return ToApplicationUser(document);
        }

        public Task<ApplicationUser> FindByUserName(string normalizedUserName)
        {
            throw new NotImplementedException();
        }

        private static ApplicationUser ToApplicationUser(Document document)
        {
            return new ApplicationUser()
            {
                NormalizedEmail = document["normalizedEmail"].AsString(),
                Email = document["email"].AsString(),
                NormalizedUserName = document["normalizedUserName"].AsString(),
                UserName = document["userName"].AsString(),
                EmailConfirmed = document["confirmed"].AsBoolean(),
                PasswordHash = document["passwordHash"].AsString()
            };
        }

    }
}
