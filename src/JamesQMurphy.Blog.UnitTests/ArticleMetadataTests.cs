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


        [Test]
        public void MonthAndYearFields()
        {
            var articleMetaData = new ArticleMetadata();

            articleMetaData.PublishDate = new System.DateTime(2012, 2, 4);
            Assert.AreEqual("2012", articleMetaData.YearString);
            Assert.AreEqual("02", articleMetaData.MonthString);

            articleMetaData.PublishDate = new System.DateTime(2034, 11, 30);
            Assert.AreEqual("2034", articleMetaData.YearString);
            Assert.AreEqual("11", articleMetaData.MonthString);
        }

        [Test]
        public void TestEquality()
        {
            var articleMetaData1 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug",
                PublishDate = new System.DateTime(2019, 6, 1)
            };

            var articleMetaData2 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug",
                PublishDate = new System.DateTime(2019, 6, 1)
            };

            Assert.AreEqual(articleMetaData1, articleMetaData2);
            Assert.IsTrue(articleMetaData1.Equals(articleMetaData2));
            Assert.IsTrue(articleMetaData1 == articleMetaData2);
            Assert.IsFalse(articleMetaData1 != articleMetaData2);
            Assert.IsTrue(articleMetaData1 <= articleMetaData2);
            Assert.IsTrue(articleMetaData1 >= articleMetaData2);

            Assert.AreEqual(articleMetaData1.GetHashCode(), articleMetaData2.GetHashCode());
        }

        [Test]
        public void TestInequality()
        {
            var articleMetaData1 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug",
                PublishDate = new System.DateTime(2019, 6, 1)
            };

            var articleMetaData2 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug2",
                PublishDate = new System.DateTime(2019, 6, 1)
            };

            Assert.AreNotEqual(articleMetaData1, articleMetaData2);
            Assert.IsFalse(articleMetaData1.Equals(articleMetaData2));
            Assert.IsFalse(articleMetaData1 == articleMetaData2);
            Assert.IsTrue(articleMetaData1 != articleMetaData2);
        }

        public void Comparison()
        {
            var articleMetaData1 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug",
                PublishDate = new System.DateTime(2019, 6, 1)
            };

            var articleMetaData2 = new ArticleMetadata()
            {
                Title = "Some Title",
                Slug = "some-slug",
                PublishDate = new System.DateTime(2019, 6, 2)
            };

            Assert.IsTrue(articleMetaData1 < articleMetaData2);
            Assert.IsTrue(articleMetaData2 > articleMetaData1);
            Assert.IsFalse(articleMetaData1 > articleMetaData2);
            Assert.IsFalse(articleMetaData2 < articleMetaData1);
            Assert.IsTrue(articleMetaData1 <= articleMetaData2);
            Assert.IsTrue(articleMetaData2 >= articleMetaData1);
            Assert.IsFalse(articleMetaData1 >= articleMetaData2);
            Assert.IsFalse(articleMetaData2 <= articleMetaData1);
        }

        [Test]
        public void Sort()
        {
            ArticleMetadata metadata0 = null;
            var metadata1 = new ArticleMetadata()
            {
                Title = "xyz",
                Slug = "xyz",
                PublishDate = new System.DateTime(2000, 1, 1)
            };
            var metadata2 = new ArticleMetadata()
            {
                Title = "abc",
                Slug = "abc",
                PublishDate = new System.DateTime(2000, 1, 2)
            };
            var metadata3 = new ArticleMetadata()
            {
                Title = "abc",
                Slug = "abc",
                PublishDate = new System.DateTime(2000, 1, 3)
            };
            var metadata4 = new ArticleMetadata()
            {
                Title = "abd",
                Slug = "abc",
                PublishDate = new System.DateTime(2000, 1, 3)
            };
            var metadata5 = new ArticleMetadata()
            {
                Title = "abc",
                Slug = "abc",
                PublishDate = new System.DateTime(2000, 1, 4)
            };
            var metadata6 = new ArticleMetadata()
            {
                Title = "abd",
                Slug = "abc",
                PublishDate = new System.DateTime(2000, 1, 4)
            };


            var arr = new ArticleMetadata[]
            {
                metadata4,
                metadata3,
                metadata1,
                metadata0,
                metadata6,
                metadata5,
                metadata2
            };

            System.Array.Sort(arr);

            Assert.AreEqual(metadata0, arr[0]);
            Assert.AreEqual(metadata1, arr[1]);
            Assert.AreEqual(metadata2, arr[2]);
            Assert.AreEqual(metadata3, arr[3]);
            Assert.AreEqual(metadata4, arr[4]);
            Assert.AreEqual(metadata5, arr[5]);
            Assert.AreEqual(metadata6, arr[6]);
        }
    }
}