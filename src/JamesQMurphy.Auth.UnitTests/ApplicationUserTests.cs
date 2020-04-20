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
        public void CanCreateAndReadFields()
        {
            var user = new ApplicationUser();

            Assert.IsNotNull(user.UserId);
            Assert.IsEmpty(user.UserName);
            Assert.IsEmpty(user.NormalizedUserName);
            Assert.IsEmpty(user.Email);
            Assert.IsEmpty(user.NormalizedEmail);
        }

        [Test]
        public void CanCreateFromRecords()
        {
            var userId = "someUserId";
            var records = new ApplicationUserRecord[]
            {
                new ApplicationUserRecord("provider1", "key1", userId),
                new ApplicationUserRecord("provider2", "key2", userId),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId)
            };
            var user = new ApplicationUser(records);
            Assert.AreEqual(userId, user.UserId);
        }

        [Test]
        public void ZeroRecordsStillHasId()
        {
            var user = new ApplicationUser(Enumerable.Empty<ApplicationUserRecord>());

            Assert.IsNotNull(user.UserId);
            Assert.IsEmpty(user.UserName);
            Assert.IsEmpty(user.NormalizedUserName);
            Assert.IsEmpty(user.Email);
            Assert.IsEmpty(user.NormalizedEmail);
        }

        [Test]
        public void NormalizedKeyForIdNotNull()
        {
            var user = new ApplicationUser();
            Assert.AreEqual(
                user.UserId,
                user.ApplicationUserRecords
                    .Where(rec => rec.Provider == ApplicationUserRecord.RECORD_TYPE_ID)
                    .First()
                    .NormalizedKey
                    );
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
            Assert.AreEqual(emailAddress, user.NormalizedEmail);
        }

        [Test]
        public void CanSetEmailAddressTwice()
        {
            var emailAddress = "test@local";
            var emailAddress2 = "test2@local";
            var user = new ApplicationUser();
            user.Email = emailAddress;
            user.Email = emailAddress2;

            Assert.IsNotNull(user.UserId);
            Assert.IsEmpty(user.UserName);
            Assert.IsEmpty(user.NormalizedUserName);
            Assert.AreEqual(emailAddress2, user.Email);
            Assert.AreEqual(emailAddress2, user.NormalizedEmail);
        }

        [Test]
        public void CannotSetNormalizedEmailAddressUntilEmailAddressSet()
        {
            var emailAddress = "test@local";
            var emailAddressNormalized = "NORMALIZED";
            var user = new ApplicationUser();

            Assert.Throws(Is.TypeOf<KeyNotFoundException>(),
                delegate
                {
                   user.NormalizedEmail = emailAddressNormalized;
                }
                );

            user.Email = emailAddress;
            user.NormalizedEmail = emailAddressNormalized;
            Assert.AreEqual(emailAddressNormalized, user.NormalizedEmail);
        }

        [Test]
        public void CanSetNormalizedEmailAddressEmpty()
        {
            var user = new ApplicationUser();
            user.NormalizedEmail = string.Empty;
            Assert.IsEmpty(user.Email);
            Assert.IsEmpty(user.NormalizedEmail);
            Assert.AreEqual(1, user.ApplicationUserRecords.Count);
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
            var stringAttributes = new Dictionary<string, string>();
            var boolAttributes = new Dictionary<string, bool>();
            boolAttributes[ApplicationUser.FIELD_ISADMINISTRATOR] = true;
            var idRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated, stringAttributes, boolAttributes);
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

        [Test]
        public void CanUpdateApplicationUserRecord()
        {
            var oldEmail = "old@local";
            var newEmail = "new@local";

            var user = new ApplicationUser
            {
                Email = oldEmail
            };
            Assert.AreEqual(oldEmail, user.Email);

            user.AddOrReplaceUserRecord(new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, newEmail, user.UserId));
            Assert.AreEqual(newEmail, user.Email);
        }

        [Test]
        public void CanCreateCleanVersionOfApplicationUserRecord()
        {
            var provider = "someProvider";
            var key = "someKey";
            var normalizedKey = "someKey normalized";
            var userId = "someUserId";
            var boolAttribute = "someFalseAttribute";
            var stringAttribute = "someStringAttribute";
            var stringAttributeValue = "some value for string attribute";

            var applicationUserRecord = new ApplicationUserRecord(provider, key, userId);
            applicationUserRecord.NormalizedKey = normalizedKey;
            applicationUserRecord.SetBoolAttribute(boolAttribute, false);
            applicationUserRecord.SetStringAttribute(stringAttribute, stringAttributeValue);
            Assert.IsTrue(applicationUserRecord.IsDirty);

            var lastUpdated = DateTime.UtcNow;
            var cleanApplicationUserRecord = ApplicationUserRecord.CreateCleanRecord(applicationUserRecord, lastUpdated);
            Assert.IsFalse(cleanApplicationUserRecord.IsDirty);
            Assert.AreEqual(lastUpdated, cleanApplicationUserRecord.LastUpdated);
            Assert.AreEqual(applicationUserRecord.Provider, cleanApplicationUserRecord.Provider);
            Assert.AreEqual(applicationUserRecord.Key, cleanApplicationUserRecord.Key);
            Assert.AreEqual(applicationUserRecord.NormalizedKey, cleanApplicationUserRecord.NormalizedKey);
            Assert.AreEqual(applicationUserRecord.BoolAttributes[boolAttribute], cleanApplicationUserRecord.BoolAttributes[boolAttribute]);
            Assert.AreEqual(applicationUserRecord.StringAttributes[stringAttribute], cleanApplicationUserRecord.StringAttributes[stringAttribute]);
        }

        [Test]
        public void SettingSameValueDoesntMakeDirty()
        {
            var provider = "someProvider";
            var key = "someKey";
            var normalizedKey = "someKey normalized";
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var boolAttribute = "someFalseAttribute";
            var stringAttribute = "someStringAttribute";
            var stringAttributeValue = "some value for string attribute";
            var applicationUserRecord = new ApplicationUserRecord(
                provider,
                key,
                userId,
                normalizedKey,
                lastUpdated,
                new Dictionary<string, string> { { stringAttribute, stringAttributeValue } },
                new Dictionary<string, bool> { { boolAttribute, false } }
                );

            Assert.IsFalse(applicationUserRecord.IsDirty);

            applicationUserRecord.NormalizedKey = normalizedKey;
            applicationUserRecord.SetBoolAttribute(boolAttribute, false);
            applicationUserRecord.SetStringAttribute(stringAttribute, stringAttributeValue);
            Assert.IsFalse(applicationUserRecord.IsDirty);
            Assert.AreEqual(lastUpdated, applicationUserRecord.LastUpdated);
        }

        [Test]
        public void SettingDifferentValueDoesMakeDirty()
        {
            var provider = "someProvider";
            var key = "someKey";
            var normalizedKey = "someKey normalized";
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var boolAttribute = "someFalseAttribute";
            var stringAttribute = "someStringAttribute";
            var stringAttributeValue = "some value for string attribute";
            var applicationUserRecord = new ApplicationUserRecord(
                provider,
                key,
                userId,
                normalizedKey,
                lastUpdated,
                new Dictionary<string, string> { { stringAttribute, stringAttributeValue } },
                new Dictionary<string, bool> { { boolAttribute, false } }
                );

            Assert.IsFalse(applicationUserRecord.IsDirty);

            applicationUserRecord.NormalizedKey = normalizedKey + "x";
            Assert.IsTrue(applicationUserRecord.IsDirty);
        }

        [Test]
        public void SettingDifferentAttributeDoesMakeDirty()
        {
            var provider = "someProvider";
            var key = "someKey";
            var normalizedKey = "someKey normalized";
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var boolAttribute = "someFalseAttribute";
            var stringAttribute = "someStringAttribute";
            var stringAttributeValue = "some value for string attribute";
            var applicationUserRecord = new ApplicationUserRecord(
                provider,
                key,
                userId,
                normalizedKey,
                lastUpdated,
                new Dictionary<string, string> { { stringAttribute, stringAttributeValue } },
                new Dictionary<string, bool> { { boolAttribute, false } }
                );

            Assert.IsFalse(applicationUserRecord.IsDirty);

            applicationUserRecord.SetStringAttribute(stringAttribute, stringAttributeValue + "x");
            Assert.IsTrue(applicationUserRecord.IsDirty);
        }

        [Test]
        public void CantInsertRecordsWithDifferentUserIds()
        {
            var user = new ApplicationUser();
            Assert.Throws(Is.TypeOf<InvalidOperationException>(),
                delegate
                {
                    user.AddOrReplaceUserRecord(new ApplicationUserRecord("provider", "key", user.UserId + "x"));
                }
                );
        }

        [Test]
        public void NormalizedKeyDefaultsToKey()
        {
            var key = "key";
            var rec = new ApplicationUserRecord("provider", "key", "userId");
            Assert.AreEqual(key, rec.NormalizedKey);
        }

        [Test]
        public void CannotReadLastUpdatedForNewUser()
        {
            var user = new ApplicationUser();
            Assert.Throws(Is.TypeOf<InvalidOperationException>(),
                delegate
                {
                    var d = user.LastUpdated;
                }
                );
        }

        [Test]
        public void CannotReadLastUpdatedForDirtyUser()
        {
            var userId = "someUserId";
            var lastUpdated = DateTime.UtcNow;
            var idRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated);
            var user = new ApplicationUser(new[] { idRecord });
            user.IsAdministrator = true;

            Assert.IsTrue(idRecord.IsDirty);
            Assert.Throws(Is.TypeOf<InvalidOperationException>(),
                delegate
                {
                    var d = user.LastUpdated;
                }
                );
        }

        [Test]
        public void LastUpdated()
        {
            var userId = "someUserId";
            var email = "someEmail@local";
            var lastUpdated = DateTime.UtcNow;
            var idRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId, lastUpdated.AddDays(-1));
            var emailRecord = new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, email, userId, email, lastUpdated);
            var user = new ApplicationUser(new[] { idRecord, emailRecord });

            Assert.IsFalse(idRecord.IsDirty);
            Assert.AreEqual(lastUpdated, user.LastUpdated);
        }

    }
}