using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;

namespace Tests
{
    public class ArticleTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreate()
        {
            Assert.IsInstanceOf<Article>(new Article());
        }

        [Test]
        public void CanSetProperties()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = "Here is some content\nwhich spans multiple lines";
            var publishDate = System.DateTime.UtcNow;

            var article = new Article()
            {
                Content = content,
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            Assert.AreEqual(title, article.Title);
            Assert.AreEqual(slug, article.Slug);
            Assert.AreEqual(content, article.Content);
            Assert.AreEqual(publishDate, article.PublishDate);
            Assert.AreEqual(string.Empty, article.Description);
        }

        [Test]
        public void ReadFromStream()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = $"Here is some content{System.Environment.NewLine}which spans multiple lines";
            var publishDate = System.DateTime.UtcNow;

            var streamString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
{content}";

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(streamString));
            var article = Article.ReadFrom(memoryStream);

            Assert.AreEqual(title, article.Title);
            Assert.AreEqual(slug, article.Slug);
            Assert.AreEqual(content.Trim(), article.Content.Trim());
            Assert.AreEqual(publishDate, article.PublishDate);
            Assert.AreEqual(string.Empty, article.Description);
        }

        [Test]
        public void ReadFromStreamWithDescription()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = $"Here is some content{System.Environment.NewLine}which spans multiple lines";
            var description = "Some kind of description";
            var publishDate = System.DateTime.UtcNow;

            var streamString = $@"---
title: {title}
description: {description}
slug: {slug}
publish-date: {publishDate:O}
...
{content}";

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(streamString));
            var article = Article.ReadFrom(memoryStream);

            Assert.AreEqual(title, article.Title);
            Assert.AreEqual(slug, article.Slug);
            Assert.AreEqual(content.Trim(), article.Content.Trim());
            Assert.AreEqual(publishDate, article.PublishDate);
            Assert.AreEqual(description, article.Description);
        }

        [Test]
        public void WriteToStream()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = $"Here is some content{System.Environment.NewLine}which spans multiple lines";
            var publishDate = System.DateTime.UtcNow;
            var article = new Article()
            {
                Content = content,
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            var memoryStream = new MemoryStream();
            article.WriteTo(memoryStream, true);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var streamString = new StreamReader(memoryStream).ReadToEnd();

            var compareString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
{content}";

            Assert.AreEqual(compareString.Trim(), streamString.Trim());
        }

        [Test]
        public void WriteToStreamWithDescription()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = $"Here is some content{System.Environment.NewLine}which spans multiple lines";
            var description = "Some kind of description";
            var publishDate = System.DateTime.UtcNow;
            var article = new Article()
            {
                Content = content,
                Slug = slug,
                Title = title,
                PublishDate = publishDate,
                Description = description
            };

            var memoryStream = new MemoryStream();
            article.WriteTo(memoryStream, true);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var streamString = new StreamReader(memoryStream).ReadToEnd();

            var compareString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
description: {description}
...
{content}";

            Assert.AreEqual(compareString.Trim(), streamString.Trim());
        }

        [Test]
        public void LeaveStreamOpen()
        {
            var memoryStreamOpen = new MemoryStream();
            (new Article()).WriteTo(memoryStreamOpen, true);
            Assert.IsTrue(memoryStreamOpen.CanWrite);

            var memoryStreamClosed = new MemoryStream();
            (new Article()).WriteTo(memoryStreamClosed, false);
            Assert.IsFalse(memoryStreamClosed.CanWrite);

        }

        [Test]
        public void CreateFromString()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = $"Here is some content{System.Environment.NewLine}which spans multiple lines";
            var publishDate = System.DateTime.UtcNow;

            var theString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
{content}";

            var article = Article.ReadFrom(theString);

            Assert.AreEqual(title, article.Title);
            Assert.AreEqual(slug, article.Slug);
            Assert.AreEqual(content.Trim(), article.Content.Trim());
            Assert.AreEqual(publishDate, article.PublishDate);
            Assert.AreEqual(string.Empty, article.Description);
        }


        [Test]
        public void TestToString()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var content = "Here is some content\nwhich spans multiple lines";
            var publishDate = System.DateTime.UtcNow;
            var article = new Article()
            {
                Content = content,
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            var compareString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
{content}";

            Assert.AreEqual(compareString, article.ToString());
        }

        [Test]
        public void MonthAndYearFields()
        {
            var article = new Article();

            article.PublishDate = new System.DateTime(2012, 2, 4);
            Assert.AreEqual("2012", article.YearString);
            Assert.AreEqual("02", article.MonthString);

            article.PublishDate = new System.DateTime(2034, 11, 30);
            Assert.AreEqual("2034", article.YearString);
            Assert.AreEqual("11", article.MonthString);
        }

    }
}