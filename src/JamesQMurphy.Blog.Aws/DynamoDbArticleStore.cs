﻿using System;
using System.Collections.Generic;
using JamesQMurphy.Blog;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;

namespace JamesQMurphy.Blog.Aws
{
    public class DynamoDbArticleStore : IArticleStore
    {
        private readonly Table table;
        private readonly List<string> metadataAttributesToGet = new List<string> { "slug", "title", "publishDate", "description" };

        public DynamoDbArticleStore(IAmazonDynamoDB dynamoDbClient, string tableName)
        {
            table = Table.LoadTable(dynamoDbClient, tableName);
        }

        public Article GetArticle(string yearString, string monthString, string slug)
        {
            var yearMonthString = $"{yearString}{monthString}";
            var result = table.GetItemAsync(yearMonthString, slug).GetAwaiter().GetResult();
            return new Article()
            {
                Slug = result["slug"],
                Title = result["title"],
                PublishDate = DateTime.Parse(result["publishDate"]).ToUniversalTime(),
                Description = result.ContainsKey("description") ? (string)result["description"] : "",
                Content = result["content"]
            };

        }

        public IEnumerable<ArticleMetadata> GetArticles(string yearString = null, string monthString = null)
        {
            var results = new List<ArticleMetadata>();

            var config = new ScanOperationConfig()
            {
                AttributesToGet = metadataAttributesToGet,
                Select = SelectValues.SpecificAttributes,
                Filter = new ScanFilter()
            };


            if (yearString != null)
            {
                if (monthString == null)
                {
                    config.Filter.AddCondition("yearMonth", ScanOperator.BeginsWith, yearString);
                }
                else
                {
                    config.Filter.AddCondition("yearMonth", ScanOperator.Equal, $"{yearString}{monthString}");
                }
            }

            var search = table.Scan(config);
            do
            {
                var documentSet = search.GetNextSetAsync().GetAwaiter().GetResult();
                results.AddRange(documentSet.ConvertAll((d) => new ArticleMetadata()
                {
                    Slug = d["slug"],
                    Title = d["title"],
                    PublishDate = DateTime.Parse(d["publishDate"]).ToUniversalTime(),
                    Description = d.ContainsKey("description") ? (string)d["description"] : ""
                }
                ));
            } while (!search.IsDone);

            results.Sort();
            return results;
        }
    }
}