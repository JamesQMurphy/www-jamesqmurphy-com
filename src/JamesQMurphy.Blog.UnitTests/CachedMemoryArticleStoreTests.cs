using NUnit.Framework;
using JamesQMurphy.Blog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    public class CachedMEmoryArticleStoreTests
    {
        private class TrackableInMemoryArticleStore : IArticleStore
        {
            public readonly InMemoryArticleStore InMemoryArticleStore = new InMemoryArticleStore();

            public int GetArticleSyncCount = 0;
            public Task<Article> GetArticleAsync(string slug)
            {
                ++GetArticleSyncCount;
                return InMemoryArticleStore.GetArticleAsync(slug);
            }
            public Task<Article> GetArticleAsync_NoCount(string slug)
            {
                return InMemoryArticleStore.GetArticleAsync(slug);
            }


            public int GetArticleMetadatasSyncCount = 0;
            public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync(DateTime startDate, DateTime endDate)
            {
                ++GetArticleMetadatasSyncCount;
                return InMemoryArticleStore.GetArticleMetadatasAsync(startDate, endDate);
            }
            public Task<IEnumerable<ArticleMetadata>> GetArticleMetadatasAsync_NoCount(DateTime startDate, DateTime endDate)
            {
                return InMemoryArticleStore.GetArticleMetadatasAsync(startDate, endDate);
            }

            public int GetLastArticlesAsyncCount = 0;
            public Task<IEnumerable<Article>> GetLastArticlesAsync(int numberOfArticles)
            {
                ++GetLastArticlesAsyncCount;
                return InMemoryArticleStore.GetLastArticlesAsync(numberOfArticles);
            }
            public Task<IEnumerable<Article>> GetLastArticlesAsync_NoCount(int numberOfArticles)
            {
                return InMemoryArticleStore.GetLastArticlesAsync(numberOfArticles);
            }

            public Task<IEnumerable<ArticleReaction>> GetArticleReactions(string articleSlug, string sinceTimestamp = "", int pageSize = 50, bool latest = false)
            {
                throw new NotImplementedException();
            }

            public Task<bool> AddReaction(string articleSlug, ArticleReactionType articleReactionType, string content, string userId, string userName, DateTime timestamp, string replyingTo = "")
            {
                throw new NotImplementedException();
            }
        }

        private TrackableInMemoryArticleStore _trackableInMemoryArticleStore;
        private CachedArticleStore<TrackableInMemoryArticleStore> _cachedArticleStore;

        [SetUp]
        public void Setup()
        {
            _trackableInMemoryArticleStore = new TrackableInMemoryArticleStore();
            _cachedArticleStore = new CachedArticleStore<TrackableInMemoryArticleStore>(_trackableInMemoryArticleStore);
        }

        [Test]
        public void GetArticleGetsCached()
        {
            var article = new Article
            {
                Title = "Some Article",
                Slug = "2019/01/some-slug",
                PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                Content = "This is an article published on January 10, 2019 at 12:34pm UTC"
            };

            _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);

            var articleFirstTime = _cachedArticleStore.GetArticleAsync(article.Slug).GetAwaiter().GetResult();
            var articleSecondTime = _cachedArticleStore.GetArticleAsync(article.Slug).GetAwaiter().GetResult();
            Assert.AreSame(article, articleFirstTime);
            Assert.AreSame(article, articleSecondTime);
            Assert.AreEqual(1, _trackableInMemoryArticleStore.GetArticleSyncCount);
        }

        [Test]
        public void SameDateRangeCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            var startDate = new DateTime(2019, 2, 1);
            var endDate = new DateTime(2019, 3, 31);
            var actualReturned = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate, endDate).GetAwaiter().GetResult());
            var returnedFirstTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate, endDate).GetAwaiter().GetResult());
            var returnedSecondTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate, endDate).GetAwaiter().GetResult());

            Assert.AreSame(actualReturned[0], returnedFirstTime[0]);
            Assert.AreSame(actualReturned[1], returnedFirstTime[1]);
            Assert.AreSame(actualReturned[0], returnedSecondTime[0]);
            Assert.AreSame(actualReturned[1], returnedSecondTime[1]);
            Assert.AreEqual(1, _trackableInMemoryArticleStore.GetArticleMetadatasSyncCount);
        }

        [Test]
        public void SmallerDateRangeCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            var startDate = new DateTime(2019, 2, 1);
            var endDate = new DateTime(2019, 3, 31);
            var actualReturned = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate, endDate).GetAwaiter().GetResult());
            var returnedFirstTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate, endDate).GetAwaiter().GetResult());
            var returnedSecondTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate.AddDays(1), endDate.AddDays(-1)).GetAwaiter().GetResult());

            Assert.AreSame(actualReturned[0], returnedFirstTime[0]);
            Assert.AreSame(actualReturned[1], returnedFirstTime[1]);
            Assert.AreSame(actualReturned[0], returnedSecondTime[0]);
            Assert.AreSame(actualReturned[1], returnedSecondTime[1]);
            Assert.AreEqual(1, _trackableInMemoryArticleStore.GetArticleMetadatasSyncCount);
        }

        [Test]
        public void LargerDateRangeCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            var startDate = new DateTime(2019, 2, 1);
            var endDate = new DateTime(2019, 3, 31);
            var actualReturned = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate, endDate).GetAwaiter().GetResult());
            var returnedFirstTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate, endDate).GetAwaiter().GetResult());
            var returnedSecondTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate.AddDays(-1), endDate.AddDays(1)).GetAwaiter().GetResult());
            var returnedThirdTime = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate.AddDays(-1), endDate.AddDays(1)).GetAwaiter().GetResult());

            Assert.AreSame(actualReturned[0], returnedFirstTime[0]);
            Assert.AreSame(actualReturned[1], returnedFirstTime[1]);
            Assert.AreSame(actualReturned[0], returnedSecondTime[0]);
            Assert.AreSame(actualReturned[1], returnedSecondTime[1]);
            Assert.AreSame(actualReturned[0], returnedThirdTime[0]);
            Assert.AreSame(actualReturned[1], returnedThirdTime[1]);
            Assert.AreEqual(2, _trackableInMemoryArticleStore.GetArticleMetadatasSyncCount);
        }

        [Test]
        public void OverlappingDateRangeCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            var startDate1 = new DateTime(2019, 2, 1);
            var endDate1 = new DateTime(2019, 3, 31);
            var actualReturned = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate1, endDate1).GetAwaiter().GetResult());
            var cachedReturned = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate1, endDate1).GetAwaiter().GetResult());

            Assert.AreEqual(actualReturned.Count, cachedReturned.Count);
            Assert.AreSame(actualReturned[0], cachedReturned[0]);
            Assert.AreSame(actualReturned[1], cachedReturned[1]);

            var startDate2 = new DateTime(2019, 3, 1);
            var endDate2 = new DateTime(2019, 4, 30);
            var actualReturned2 = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate2, endDate2).GetAwaiter().GetResult());
            var cachedReturned2 = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate2, endDate2).GetAwaiter().GetResult());
            Assert.AreEqual(actualReturned2.Count, cachedReturned2.Count);
            Assert.AreSame(actualReturned2[0], cachedReturned2[0]);
            Assert.AreSame(actualReturned2[1], cachedReturned2[1]);

            var actualReturned3 = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate1, endDate2).GetAwaiter().GetResult());
            var cachedReturned3 = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate1, endDate2).GetAwaiter().GetResult());
            Assert.AreEqual(actualReturned3.Count, cachedReturned3.Count);
            Assert.AreSame(actualReturned3[0], cachedReturned3[0]);
            Assert.AreSame(actualReturned3[1], cachedReturned3[1]);
            Assert.AreSame(actualReturned3[2], cachedReturned3[2]);

            Assert.AreEqual(2, _trackableInMemoryArticleStore.GetArticleMetadatasSyncCount);
        }

        [Test]
        public void NonOverlappingDateRangeNotCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            var startDate1 = new DateTime(2019, 2, 1);
            var endDate1 = new DateTime(2019, 2, 28);
            var actualReturned = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate1, endDate1).GetAwaiter().GetResult());
            var cachedReturned = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate1, endDate1).GetAwaiter().GetResult());

            Assert.AreEqual(actualReturned.Count, cachedReturned.Count);
            Assert.AreSame(actualReturned[0], cachedReturned[0]);

            var startDate2 = new DateTime(2019, 4, 1);
            var endDate2 = new DateTime(2019, 4, 30);
            var actualReturned2 = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate2, endDate2).GetAwaiter().GetResult());
            var cachedReturned2 = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate2, endDate2).GetAwaiter().GetResult());
            Assert.AreEqual(actualReturned2.Count, cachedReturned2.Count);
            Assert.AreSame(actualReturned2[0], cachedReturned2[0]);

            var actualReturned3 = new List<ArticleMetadata>(_trackableInMemoryArticleStore.GetArticleMetadatasAsync_NoCount(startDate1, endDate2).GetAwaiter().GetResult());
            var cachedReturned3 = new List<ArticleMetadata>(_cachedArticleStore.GetArticleMetadatasAsync(startDate1, endDate2).GetAwaiter().GetResult());
            Assert.AreEqual(actualReturned3.Count, cachedReturned3.Count);
            Assert.AreSame(actualReturned3[0], cachedReturned3[0]);
            Assert.AreSame(actualReturned3[1], cachedReturned3[1]);
            Assert.AreSame(actualReturned3[2], cachedReturned3[2]);

            Assert.AreEqual(3, _trackableInMemoryArticleStore.GetArticleMetadatasSyncCount);
        }

        [Test]
        public void LastNumberArticlesCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            int numberOfArticlesToFetch = 2;
            var actualReturned = new List<Article>(_trackableInMemoryArticleStore.GetLastArticlesAsync_NoCount(numberOfArticlesToFetch).GetAwaiter().GetResult());
            var returnedFirstTime = new List<Article>(_cachedArticleStore.GetLastArticlesAsync(numberOfArticlesToFetch).GetAwaiter().GetResult());
            var returnedSecondTime = new List<Article>(_cachedArticleStore.GetLastArticlesAsync(numberOfArticlesToFetch).GetAwaiter().GetResult());

            Assert.AreEqual(numberOfArticlesToFetch, actualReturned.Count);
            Assert.AreEqual(numberOfArticlesToFetch, returnedFirstTime.Count);
            Assert.AreEqual(numberOfArticlesToFetch, returnedSecondTime.Count);
            Assert.AreSame(actualReturned[0], returnedFirstTime[0]);
            Assert.AreSame(actualReturned[1], returnedFirstTime[1]);
            Assert.AreSame(actualReturned[0], returnedSecondTime[0]);
            Assert.AreSame(actualReturned[1], returnedSecondTime[1]);
            Assert.AreEqual(1, _trackableInMemoryArticleStore.GetLastArticlesAsyncCount);
        }

        [Test]
        public void GetArticleUsesLastNumberArticlesCached()
        {
            var articles = new Article[]
            {
                new Article
                {
                    Title = "January",
                    Slug = "2019/01/january",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is January"
                },
                new Article
                {
                    Title = "February",
                    Slug = "2019/02/february",
                    PublishDate = new DateTime(2019, 2, 10, 12, 34, 56),
                    Content = "This is February"
                },
                new Article
                {
                    Title = "March",
                    Slug = "2019/03/march",
                    PublishDate = new DateTime(2019, 3, 10, 12, 34, 56),
                    Content = "This is March"
                },
                new Article
                {
                    Title = "April",
                    Slug = "2019/04/april",
                    PublishDate = new DateTime(2019, 4, 10, 12, 34, 56),
                    Content = "This is April"
                }
            };

            foreach (var article in articles)
            {
                _trackableInMemoryArticleStore.InMemoryArticleStore.SafeAddArticle(article);
            }

            int numberOfArticlesToFetch = 2;
            var actualReturned = new List<Article>(_cachedArticleStore.GetLastArticlesAsync(numberOfArticlesToFetch).GetAwaiter().GetResult());

            var article0 = _cachedArticleStore.GetArticleAsync(actualReturned[0].Slug).GetAwaiter().GetResult();
            var article1 = _cachedArticleStore.GetArticleAsync(actualReturned[1].Slug).GetAwaiter().GetResult();

            Assert.AreSame(actualReturned[0], article0);
            Assert.AreSame(actualReturned[1], article1);
            Assert.AreEqual(0, _trackableInMemoryArticleStore.GetArticleSyncCount);
        }
    }


}