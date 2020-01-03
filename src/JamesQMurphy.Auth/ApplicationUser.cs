using System;
using System.Collections.Generic;

namespace JamesQMurphy.Auth
{
    public class ApplicationUser
    {
        public const string FIELD_PASSWORDHASH = "passwordHash";
        public const string FIELD_EMAILCONFIRMED = "emailConfirmed";
        public const string FIELD_ISADMINISTRATOR = "isAdministrator";

        private readonly Dictionary<string, ApplicationUserRecord> records = new Dictionary<string, ApplicationUserRecord>();

        public ApplicationUser()
        {
            // Create a single ApplicationUserRecord to represent the ID
            var miniGuid = NewMiniGuid();
            records.Add(ApplicationUserRecord.RECORD_TYPE_ID, new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, miniGuid, miniGuid));
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
            set => CreateUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, value);
        }

        public string NormalizedEmail
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_EMAIL)?.NormalizedKey ?? "";
            set
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                rec.NormalizedKey = value;
            }
        }

        public string UserName
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_USERNAME)?.Key ?? "";
            set => CreateUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_USERNAME, value);
        }

        public string NormalizedUserName
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_USERNAME)?.NormalizedKey ?? "";
            set
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_USERNAME, nameof(UserName));
                rec.NormalizedKey = value;
            }
        }

        public bool EmailConfirmed
        {
            get
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                return rec.BoolAttributes.ContainsKey(FIELD_EMAILCONFIRMED) ? rec.BoolAttributes[FIELD_EMAILCONFIRMED] : false;
            }
            set
            {
                var rec = GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAIL, nameof(Email));
                rec.SetBoolAttribute(FIELD_EMAILCONFIRMED, value);
            }
        }

        public string PasswordHash
        {
            get => GetStringAttribute(ApplicationUserRecord.RECORD_TYPE_EMAIL, FIELD_PASSWORDHASH);
            set => SetStringAttribute(ApplicationUserRecord.RECORD_TYPE_EMAIL, FIELD_PASSWORDHASH, nameof(Email), value);
        }

        public bool IsAdministrator
        {
            get => GetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_ISADMINISTRATOR);
            set => SetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_ISADMINISTRATOR, nameof(UserId), value);
        }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;


        private ApplicationUserRecord GetUserRecordOrNull(string recordType)
        {
            if (records.ContainsKey(recordType))
                return records[recordType];
            else
                return null;
        }

        private void CreateUserRecordOrThrow(string recordType, string value)
        {
            if (records.ContainsKey(recordType))
            {
                throw new Exception($"Cannot create recordType {recordType} more than once");
            }
            else
            {
                var rec = new ApplicationUserRecord(recordType, value, this.UserId);
                records.Add(recordType, rec);
            }
        }

        private ApplicationUserRecord GetUserRecordOrThrow(string recordType, string requiredProperty)
        {
            if (records.ContainsKey(recordType))
                return records[recordType];
            else
                throw new KeyNotFoundException($"No record type {recordType}; must set property {requiredProperty} first");
        }

        public string GetStringAttribute(string recordType, string attributeName)
        {
            if (records.ContainsKey(recordType))
            {
                if (records[recordType].StringAttributes.ContainsKey(attributeName))
                {
                    return records[recordType].StringAttributes[attributeName];
                }
            }
            return string.Empty;
        }
        public void SetStringAttribute(string recordType, string attributeName, string requiredProperty, string value)
        {
            GetUserRecordOrThrow(recordType, requiredProperty).SetStringAttribute(attributeName, value);
        }

        public bool GetBoolAttribute(string recordType, string attributeName)
        {
            if (records.ContainsKey(recordType))
            {
                if (records[recordType].BoolAttributes.ContainsKey(attributeName))
                {
                    return records[recordType].BoolAttributes[attributeName];
                }
            }
            return default;
        }
        public void SetBoolAttribute(string recordType, string attributeName, string requiredProperty, bool value)
        {
            GetUserRecordOrThrow(recordType, requiredProperty).SetBoolAttribute(attributeName, value);
        }

        private static string NewMiniGuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", String.Empty);
        }
    }
}
