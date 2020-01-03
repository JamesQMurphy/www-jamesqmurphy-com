using JamesQMurphy.Auth;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace JamesQMurphy.Web.UnitTests
{
    public class ApplicationUserTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreate()
        {
            var user = new ApplicationUser();

            Assert.IsNotNull(user.UserId);
            Assert.IsEmpty(user.UserName);
            Assert.IsEmpty(user.NormalizedUserName);
            Assert.IsEmpty(user.Email);
            Assert.IsEmpty(user.NormalizedEmail);
        }

        [Test]
        public void CanSetEmailAddress()
        {
            var emailAddress = "test@local";
            var user = new ApplicationUser();
            user.Email = emailAddress;

            Assert.IsNotNull(user.UserId);
            Assert.IsEmpty(user.UserName);
            Assert.IsEmpty(user.NormalizedUserName);
            Assert.AreEqual(emailAddress, user.Email);
            Assert.IsEmpty(user.NormalizedEmail);
        }

        [Test]
        public void CannotSetNormalizedEmailAddress()
        {
            var emailAddress = "test@local";
            var user = new ApplicationUser();

            Assert.Throws(Is.TypeOf<KeyNotFoundException>(),
                delegate
                {
                   user.NormalizedEmail = emailAddress.ToUpperInvariant();
                }
                );
        }

        [Test]
        public void CanReadButNotSetPasswordHash()
        {
            var pwhash = "something-hashy";
            var user = new ApplicationUser();
            Assert.IsEmpty(user.PasswordHash);
            Assert.Throws(Is.TypeOf<KeyNotFoundException>(),
                delegate
                {
                    user.PasswordHash = pwhash;
                }
                );

            user.Email = "something@local";
            user.PasswordHash = pwhash;
            Assert.AreEqual(pwhash, user.PasswordHash);
        }

        [Test]
        public void CanReadAndSetIsAdministrator()
        {
            var user = new ApplicationUser();
            Assert.IsFalse(user.IsAdministrator);
            user.IsAdministrator = true;
            Assert.IsTrue(user.IsAdministrator);
            user.IsAdministrator = false;
            Assert.IsFalse(user.IsAdministrator);
        }

        [Test]
        public void NewObjectDirty()
        {
            Assert.IsTrue((new ApplicationUser()).ApplicationUserRecords.First().IsDirty);
        }

        [Test]
        public void LoadedObjectNotDirty()
        {
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var idRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated);
            idRecord.BoolAttributes["IsAdministrator"] = true;
            var user = new ApplicationUser(new[] { idRecord });

            Assert.IsTrue(user.IsAdministrator);
            Assert.IsFalse(idRecord.IsDirty);
        }

        [Test]
        public void ModifiedObjectDirty()
        {
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var idRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated);
            var user = new ApplicationUser(new[] { idRecord });
            user.IsAdministrator = true;

            Assert.IsTrue(user.IsAdministrator);
            Assert.IsTrue(idRecord.IsDirty);

        }

        // TODO: PasswordHash saved in both email and username fields
    }
}