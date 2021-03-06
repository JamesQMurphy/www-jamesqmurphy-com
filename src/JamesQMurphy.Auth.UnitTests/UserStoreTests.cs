﻿using JamesQMurphy.Auth;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
                new ApplicationUserRecord(
                    ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated, new Dictionary<string,string>{
                        { ApplicationUser.FIELD_USERNAME, "OrdinaryUser" }, { ApplicationUser.FIELD_NORMALIZEDUSERNAME, "ORDINARYUSER" }
                    }, new Dictionary<string,bool>()),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, "user@local",  userId, "USER@LOCAL", lastUpdated)
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
                new ApplicationUserRecord(
                    ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated, new Dictionary<string,string>{
                        { ApplicationUser.FIELD_USERNAME, "OrdinaryUser" }, { ApplicationUser.FIELD_NORMALIZEDUSERNAME, "ORDINARYUSER" }
                    }, new Dictionary<string,bool>()),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, email,  userId, email, lastUpdated)
            };
            foreach (var rec in records)
            {
                _inMemoryApplicationUserStorage.SaveAsync(rec);
            }

            var user = _applicationUserStore.FindByEmailAddress(email).GetAwaiter().GetResult();
            Assert.AreEqual(userId, user.UserId);
        }

        [Test]
        public void GetAll_EmptyList()
        {
            Assert.IsEmpty(_applicationUserStore.GetAll().GetAwaiter().GetResult());
        }

        [Test]
        public void GetAll_WithUsers()
        {
            ApplicationUser[] users = new ApplicationUser[]
            {
                new ApplicationUser
                {
                    UserName = "user1",
                    Email = "user1email"
                },
                new ApplicationUser
                {
                    UserName = "user2",
                    Email = "user2email"
                },
                new ApplicationUser
                {
                    UserName = "user3",
                    Email = "user3email"
                },

            };

            Dictionary<string, ApplicationUser> dictUsers = new Dictionary<string, ApplicationUser>();
            foreach(var user in users)
            {
                dictUsers[user.UserName] = user;
                _applicationUserStore.CreateAsync(user).Wait();

            }

            var usersFromStore = new List<ApplicationUser>( _applicationUserStore.GetAll().GetAwaiter().GetResult());
            Assert.AreEqual(dictUsers.Count, usersFromStore.Count);
            foreach(var userFromStore in usersFromStore)
            {
                Assert.IsTrue(dictUsers.ContainsKey(userFromStore.UserName));
                Assert.AreEqual(dictUsers[userFromStore.UserName].Email, userFromStore.Email);
            }

        }

        [Test]
        public void ExternalLogins_EmptyUser()
        {
            var user = new ApplicationUser();
            var logins = _applicationUserStore.GetLoginsAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsEmpty(logins);
        }

        [Test]
        public void ExternalLogins_EmailUser()
        {
            var user = new ApplicationUser
            {
                UserName = "someUser",
                Email = "someEmail@local"
            };
            var logins = _applicationUserStore.GetLoginsAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsEmpty(logins);
        }

        [Test]
        public void ExternalLogins_ExternalUser()
        {
            var user = new ApplicationUser
            {
                UserName = "someUser"
            };
            var provider = "someProvider";
            var providerKey = "someProviderKey";
            var providerDisplayName = "Some Provider Display Name";
            var userLoginInfo = new Microsoft.AspNetCore.Identity.UserLoginInfo(provider, providerKey, providerDisplayName);
            _applicationUserStore.AddLoginAsync(user, userLoginInfo, CancellationToken.None).GetAwaiter().GetResult();
            _ = _applicationUserStore.UpdateAsync(user).GetAwaiter().GetResult();

            var loginsReturned = _applicationUserStore.GetLoginsAsync(user, CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(1, loginsReturned.Count);
            Assert.AreEqual(provider, loginsReturned[0].LoginProvider);
            Assert.AreEqual(providerKey, loginsReturned[0].ProviderKey);
            Assert.AreEqual(providerDisplayName, loginsReturned[0].ProviderDisplayName);

            _applicationUserStore.RemoveLoginAsync(user, provider, providerKey, CancellationToken.None).GetAwaiter().GetResult();
            _ = _applicationUserStore.UpdateAsync(user).GetAwaiter().GetResult();
            var userReturned = _applicationUserStore.FindById(user.UserId).GetAwaiter().GetResult();

            var loginsReturnedAfterRemove = _applicationUserStore.GetLoginsAsync(userReturned, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsEmpty(loginsReturnedAfterRemove);
        }



    }
}