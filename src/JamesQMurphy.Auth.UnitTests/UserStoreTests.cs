using JamesQMurphy.Auth;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;


namespace JamesQMurphy.Web.UnitTests
{
    public class UserStoreTests
    {
        private ApplicationUserStore _applicationUserStore;
        private InMemoryApplicationUserStorage _inMemoryApplicationUserStorage;

        [SetUp]
        public void Setup()
        {
            _inMemoryApplicationUserStorage = new InMemoryApplicationUserStorage();
            var userRecords = _inMemoryApplicationUserStorage.GetAllUserRecordsAsync().GetAwaiter().GetResult().ToList();
            foreach (var user in userRecords)
            {
                _inMemoryApplicationUserStorage.DeleteAsync(user);
            }

            _applicationUserStore = new ApplicationUserStore(_inMemoryApplicationUserStorage);
        }

        [Test]
        public void FindById_DoesNotExist()
        {
            var user = _applicationUserStore.FindById("doesnotexist").GetAwaiter().GetResult();
            Assert.IsNull(user);
        }

        [Test]
        public void FindByEmail_DoesNotExist()
        {
            var user = _applicationUserStore.FindByEmailAddress("DOESNOTEXIST@ABC.COM").GetAwaiter().GetResult();
            Assert.IsNull(user);
        }

        [Test]
        public void FindByUsername_DoesNotExist()
        {
            var user = _applicationUserStore.FindByUserName("DOESNOTEXIST").GetAwaiter().GetResult();
            Assert.IsNull(user);
        }

        [Test]
        public void FindById_FromRecords()
        {
            var userId = "someRandomString";
            var lastUpdated = DateTime.UtcNow;
            var records = new[]{
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, "user@local", "USER@LOCAL", userId, lastUpdated),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, "OrdinaryUser", "ORDINARYUSER", userId, lastUpdated)
            };
            foreach (var rec in records)
            {
                _inMemoryApplicationUserStorage.SaveAsync(rec);
            }

            var user = _applicationUserStore.FindById(userId).GetAwaiter().GetResult();
            Assert.AreEqual(userId, user.UserId);
        }

        [Test]
        public void FindByEmail_FromRecords()
        {
            var userId = "someRandomString";
            var email = "someemail@local";
            var lastUpdated = DateTime.UtcNow;
            var records = new[]{
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, email, email, userId, lastUpdated),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, "OrdinaryUser", "ORDINARYUSER", userId, lastUpdated)
            };
            foreach (var rec in records)
            {
                _inMemoryApplicationUserStorage.SaveAsync(rec);
            }

            var user = _applicationUserStore.FindByEmailAddress(email).GetAwaiter().GetResult();
            Assert.AreEqual(userId, user.UserId);
        }

    }
}