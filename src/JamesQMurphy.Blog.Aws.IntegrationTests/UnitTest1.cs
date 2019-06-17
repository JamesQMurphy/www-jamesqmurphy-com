using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon.DynamoDBv2;
using JamesQMurphy.Blog;
using JamesQMurphy.Blog.Aws;
using System.Collections.Generic;

namespace JamesQMurphy.Blog.Aws.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        private string key = "AKIASGMHMGFFL5OEQ5H4";
        private string secret = "redacted";
        [TestMethod]
        public void TestGetArticle()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var article = articleStore.GetArticle("2019", "06", "technology-choices");
                Assert.IsNotNull(article);
                Assert.IsNotNull(article.Content);
            }
            
        }

        [TestMethod]
        public void TestGetArticles1()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var articleList = articleStore.GetArticles();
                List<ArticleMetadata> lst = new List<ArticleMetadata>(articleList);
                Assert.AreEqual(4, lst.Count);
            }
        }

        [TestMethod]
        public void TestGetArticles2()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var articleList = articleStore.GetArticles("2017");
                List<ArticleMetadata> lst = new List<ArticleMetadata>(articleList);
                Assert.AreEqual(0, lst.Count);
            }
        }

        [TestMethod]
        public void TestGetArticles3()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var articleList = articleStore.GetArticles("2019");
                List<ArticleMetadata> lst = new List<ArticleMetadata>(articleList);
                Assert.AreEqual(3, lst.Count);
            }
        }

        [TestMethod]
        public void TestGetArticles4()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var articleList = articleStore.GetArticles("2019", "05");
                List<ArticleMetadata> lst = new List<ArticleMetadata>(articleList);
                Assert.AreEqual(2, lst.Count);
            }
        }

        [TestMethod]
        public void TestGetArticles5()
        {
            using (var client = new AmazonDynamoDBClient(key, secret, Amazon.RegionEndpoint.USEast1))
            {
                var articleStore = new DynamoDbArticleStore(client);
                var articleList = articleStore.GetArticles("2018");
                List<ArticleMetadata> lst = new List<ArticleMetadata>(articleList);
                Assert.AreEqual(1, lst.Count);
            }
        }

    }
}
