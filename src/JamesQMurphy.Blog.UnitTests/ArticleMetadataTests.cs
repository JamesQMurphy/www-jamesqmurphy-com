using System.IO;
using NUnit.Framework;
using JamesQMurphy.Blog;
using System.Text;

namespace Tests
{
    public class ArticleMetadataTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreate()
        {
            Assert.IsInstanceOf<ArticleMetadata>(new ArticleMetadata());
        }

        [Test]
        public void CanSetProperties()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var publishDate = System.DateTime.UtcNow;

            var articleMetadata = new ArticleMetadata()
            {
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            Assert.AreEqual(title, articleMetadata.Title);
            Assert.AreEqual(slug, articleMetadata.Slug);
            Assert.AreEqual(publishDate, articleMetadata.PublishDate);
        }

        [Test]
        public void ReadFromStream()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var publishDate = System.DateTime.UtcNow;

            var streamString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
";

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(streamString));
            var articleMetadata = ArticleMetadata.ReadFrom(memoryStream);

            Assert.AreEqual(title, articleMetadata.Title);
            Assert.AreEqual(slug, articleMetadata.Slug);
            Assert.AreEqual(publishDate, articleMetadata.PublishDate);
        }

        [Test]
        public void WriteToStream()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var publishDate = System.DateTime.UtcNow;
            var articleMetadata = new ArticleMetadata()
            {
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            var memoryStream = new MemoryStream();
            articleMetadata.WriteTo(memoryStream, true);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var streamString = new StreamReader(memoryStream).ReadToEnd();

            var compareString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
";

            Assert.AreEqual(compareString, streamString);
        }

        [Test]
        public void LeaveStreamOpen()
        {
            var memoryStreamOpen = new MemoryStream();
            (new ArticleMetadata()).WriteTo(memoryStreamOpen, true);
            Assert.IsTrue(memoryStreamOpen.CanWrite);

            var memoryStreamClosed = new MemoryStream();
            (new ArticleMetadata()).WriteTo(memoryStreamClosed, false);
            Assert.IsFalse(memoryStreamClosed.CanWrite);

        }

        [Test]
        public void CreateFromString()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var publishDate = System.DateTime.UtcNow;

            var theString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
";

            var articleMetadata = ArticleMetadata.ReadFrom(theString);

            Assert.AreEqual(title, articleMetadata.Title);
            Assert.AreEqual(slug, articleMetadata.Slug);
            Assert.AreEqual(publishDate, articleMetadata.PublishDate);
        }


        [Test]
        public void TestToString()
        {
            var title = "Some Title";
            var slug = "Some-Slug";
            var publishDate = System.DateTime.UtcNow;
            var articleMetadata = new ArticleMetadata()
            {
                Slug = slug,
                Title = title,
                PublishDate = publishDate
            };

            var compareString = $@"---
title: {title}
slug: {slug}
publish-date: {publishDate:O}
...
";

            Assert.AreEqual(compareString, articleMetadata.ToString());
        }

    }
}