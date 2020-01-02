using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace JamesQMurphy.Auth
{
    public class InMemoryApplicationUserStorage : IApplicationUserStorage
    {
        //private readonly Dictionary<string, ApplicationUser> _dictUsers = new Dictionary<string, ApplicationUser>();
        private readonly Dictionary<(string,string), ApplicationUserRecord> _dictByProviderAndNormalizedKey = new Dictionary<(string,string), ApplicationUserRecord>();
        private readonly Dictionary<string, Dictionary<string, ApplicationUserRecord>> _dictByUserIdAndProvider = new Dictionary<string, Dictionary<string, ApplicationUserRecord>>();


        public InMemoryApplicationUserStorage()
        {
            var userId = "+++User+++";
            var user = new ApplicationUser(new[]{
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, userId, userId, userId),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, "user@local", "USER@LOCAL", userId),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, "OrdinaryUser", "ORDINARYUSER", userId)
            });
            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "abcde");
            foreach (var rec in user.ApplicationUserRecords)
            {
                _ = SaveAsync(rec);
            }

            var adminUserId = "x+xAdminx+x";
            var adminUser = new ApplicationUser(new[]{
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, adminUserId, adminUserId, adminUserId),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, "admin@local", "ADMIN@LOCAL", adminUserId),
                new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, "TheAdministrator", "THEADMINISTRATOR", adminUserId)
            });
            adminUser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "abcde");
            foreach (var rec in adminUser.ApplicationUserRecords)
            {
                _ = SaveAsync(rec);
            }

        }

        public Task<ApplicationUserRecord> SaveAsync(ApplicationUserRecord applicationUserRecord, CancellationToken cancellationToken = default(CancellationToken))
        {
            _dictByProviderAndNormalizedKey[(applicationUserRecord.Provider, applicationUserRecord.NormalizedKey)] = applicationUserRecord;
            if (!_dictByUserIdAndProvider.TryGetValue(applicationUserRecord.UserId, out Dictionary<string, ApplicationUserRecord> dictRec))
            {
                dictRec = new Dictionary<string, ApplicationUserRecord>();
                _dictByUserIdAndProvider.Add(applicationUserRecord.UserId, dictRec);
            }
            dictRec[applicationUserRecord.Provider] = applicationUserRecord;
            return Task.FromResult(applicationUserRecord);
        }

        public Task<ApplicationUserRecord> DeleteAsync(ApplicationUserRecord applicationUserRecord, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictByProviderAndNormalizedKey.ContainsKey((applicationUserRecord.Provider, applicationUserRecord.NormalizedKey)))
            {
                _dictByProviderAndNormalizedKey.Remove((applicationUserRecord.Provider, applicationUserRecord.NormalizedKey));
            }
            if (_dictByUserIdAndProvider.TryGetValue(applicationUserRecord.UserId, out Dictionary<string, ApplicationUserRecord> dictRec))
            {
                if (dictRec.ContainsKey(applicationUserRecord.Provider))
                {
                    dictRec.Remove(applicationUserRecord.Provider);
                }
            }
            return Task.FromResult(applicationUserRecord);
        }


        public Task<IEnumerable<ApplicationUserRecord>> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictByUserIdAndProvider.TryGetValue(userId, out Dictionary<string, ApplicationUserRecord> dictRec))
            {
                return Task.FromResult(dictRec.Values.AsEnumerable());
            }
            else
            {
                return Task.FromResult(Enumerable.Empty<ApplicationUserRecord>());
            }
        }

        public Task<IEnumerable<ApplicationUserRecord>> FindByEmailAddressAsync(string normalizedEmailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictByProviderAndNormalizedKey.ContainsKey((ApplicationUserRecord.RECORD_TYPE_EMAIL, normalizedEmailAddress)))
            {
                return FindByIdAsync(_dictByProviderAndNormalizedKey[(ApplicationUserRecord.RECORD_TYPE_EMAIL, normalizedEmailAddress)].UserId, cancellationToken);
            }
            else
            {
                return Task.FromResult(Enumerable.Empty<ApplicationUserRecord>());
            }
        }

        public Task<IEnumerable<ApplicationUserRecord>> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictByProviderAndNormalizedKey.ContainsKey((ApplicationUserRecord.RECORD_TYPE_USERNAME, normalizedUserName)))
            {
                return FindByIdAsync(_dictByProviderAndNormalizedKey[(ApplicationUserRecord.RECORD_TYPE_USERNAME, normalizedUserName)].UserId, cancellationToken);
            }
            else
            {
                return Task.FromResult(Enumerable.Empty<ApplicationUserRecord>());
            }
        }

        public Task<IEnumerable<ApplicationUserRecord>> GetAllUserRecordsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_dictByProviderAndNormalizedKey.Values.AsEnumerable());
        }
    }
}
