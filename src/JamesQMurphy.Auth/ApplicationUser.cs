using System;
using System.Collections.Generic;

namespace JamesQMurphy.Auth
{
    public class ApplicationUser
    {
        private readonly Dictionary<string, ApplicationUserRecord> records = new Dictionary<string, ApplicationUserRecord>();

        public ApplicationUser()
        {
            // Create a single ApplicationUserRecord to represent the ID
            var miniGuid = NewMiniGuid();
            records.Add(ApplicationUserRecord.RECORD_TYPE_ID, new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, miniGuid, miniGuid, miniGuid));
        }

        public ApplicationUser(IEnumerable<ApplicationUserRecord> applicationUserRecords)
        {
            foreach(var rec in applicationUserRecords)
            {
                records.Add(rec.Provider, rec);
            }
        }

        public ICollection<ApplicationUserRecord> ApplicationUserRecords => records.Values;

        public string UserId => records[ApplicationUserRecord.RECORD_TYPE_ID].Key;

        public string Email
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_EMAIL)?.Key ?? "";
            set
            {
                var rec = GetOrCreateUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, value);
                rec.Key = value;
                rec.NormalizedKey = value;
            }
        }

        public string NormalizedEmail
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_EMAIL)?.NormalizedKey ?? "";
            set
            {
                var rec = GetOrCreateUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAIL, value);
                rec.NormalizedKey = value;
            }
        }

        public string UserName
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_USERNAME)?.Key ?? "";
            set
            {
                var rec = GetOrCreateUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, value);
                rec.Key = value;
                rec.NormalizedKey = value;
            }
        }

        public string NormalizedUserName
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_USERNAME)?.NormalizedKey ?? "";
            set
            {
                var rec = GetOrCreateUserRecord(ApplicationUserRecord.RECORD_TYPE_USERNAME, value);
                rec.NormalizedKey = value;
            }
        }

        public bool EmailConfirmed
        {
            get
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                return rec.BoolAttributes.ContainsKey("EmailConfirmed") ? rec.BoolAttributes["EmailConfirmed"] : false;
            }
            set
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                rec.BoolAttributes["EmailConfirmed"] = value;
            }
        }

        public string PasswordHash
        {
            get
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                return rec.StringAttributes.ContainsKey("PasswordHash") ? rec.StringAttributes["PasswordHash"] : "";
            }
            set
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                rec.StringAttributes["PasswordHash"] = value;
            }
        }


        private ApplicationUserRecord GetUserRecordOrNull(string recordType)
        {
            if (records.ContainsKey(recordType))
                return records[recordType];
            else
                return null;
        }

        private ApplicationUserRecord GetOrCreateUserRecord(string recordType, string defaultValue)
        {
            if (!records.TryGetValue(recordType, out ApplicationUserRecord rec))
            {
                rec = new ApplicationUserRecord(recordType, defaultValue, defaultValue, this.UserId);
                records.Add(recordType, rec);
            }
            return rec;
        }

        private ApplicationUserRecord GetUserRecordOrThrow(string recordType, string requiredProperty)
        {
            if (records.ContainsKey(recordType))
                return records[recordType];
            else
                throw new KeyNotFoundException($"No record type {recordType}; must set property {requiredProperty} first");
        }

        public bool IsAdministrator
        {
            get => records[ApplicationUserRecord.RECORD_TYPE_ID].BoolAttributes.ContainsKey("IsAdministrator") ? records[ApplicationUserRecord.RECORD_TYPE_ID].BoolAttributes["IsAdministrator"] : false;
            set => records[ApplicationUserRecord.RECORD_TYPE_ID].BoolAttributes["IsAdministrator"] = value;
        }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        private static string NewMiniGuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", String.Empty);
        }
    }
}
