using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Auth
{
    public class ApplicationUser
    {
        public const string FIELD_USERNAME = "username";
        public const string FIELD_NORMALIZEDUSERNAME = "normalizedUsername";
        public const string FIELD_PASSWORDHASH = "passwordHash";
        public const string FIELD_EMAILCONFIRMED = "emailConfirmed";
        public const string FIELD_ISADMINISTRATOR = "isAdministrator";
        public const string FIELD_PROVIDERDISPLAYNAME = "providerDisplayName";

        private readonly Dictionary<string, ApplicationUserRecord> records = new Dictionary<string, ApplicationUserRecord>();

        public ApplicationUser()
        {
            // Create a single ApplicationUserRecord to represent the ID
            var miniGuid = NewMiniGuid();
            AddOrReplaceUserRecord(new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, miniGuid, miniGuid));
        }

        public ApplicationUser(IEnumerable<ApplicationUserRecord> applicationUserRecords)
        {
            foreach(var record in applicationUserRecords)
            {
                if ((records.Count == 0) && (record.Provider != ApplicationUserRecord.RECORD_TYPE_ID))
                {
                    AddOrReplaceUserRecord(new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, record.UserId, record.UserId));
                }
                AddOrReplaceUserRecord(record);
            }
            if (records.Count == 0)
            {
                // Create a single ApplicationUserRecord to represent the ID
                var miniGuid = NewMiniGuid();
                AddOrReplaceUserRecord(new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_ID, miniGuid, miniGuid));
            }
        }

        public IReadOnlyCollection<ApplicationUserRecord> ApplicationUserRecords => records.Values;

        public string UserId => records.Values.First().UserId;

        public string Email
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER)?.Key ?? "";
            set
            {
                if (Email != value)
                {
                    AddOrReplaceUserRecord(new ApplicationUserRecord(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, value, UserId));
                }
            }
        }

        public string NormalizedEmail
        {
            get => GetUserRecordOrNull(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER)?.NormalizedKey ?? "";
            set
            {
                if (NormalizedEmail != value)
                {
                    GetUserRecordOrThrow(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, nameof(Email)).NormalizedKey = value;
                }
            }
        }

        public string UserName
        {
            get => GetStringAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_USERNAME);
            set => SetStringAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_USERNAME, "", value);
        }

        public string NormalizedUserName
        {
            get => GetStringAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_NORMALIZEDUSERNAME);
            set => SetStringAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_NORMALIZEDUSERNAME, "", value);
        }

        public bool EmailConfirmed
        {
            get => GetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, FIELD_EMAILCONFIRMED);
            set => SetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, FIELD_EMAILCONFIRMED, nameof(Email), value);
        }

        public string PasswordHash
        {
            get => GetStringAttribute(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, FIELD_PASSWORDHASH);
            set => SetStringAttribute(ApplicationUserRecord.RECORD_TYPE_EMAILPROVIDER, FIELD_PASSWORDHASH, nameof(Email), value);
        }

        public bool IsAdministrator
        {
            get => GetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_ISADMINISTRATOR);
            set => SetBoolAttribute(ApplicationUserRecord.RECORD_TYPE_ID, FIELD_ISADMINISTRATOR, "", value);
        }

        public void AddOrReplaceUserRecord(ApplicationUserRecord applicationUserRecord)
        {
            if (records.Count > 0)
            {
                if (applicationUserRecord.UserId != this.UserId)
                {
                    throw new InvalidOperationException($"Could not add applicationUserRecord with UserId={applicationUserRecord.UserId} to user with UserId={this.UserId}");
                }
            }
            records[applicationUserRecord.Provider] = applicationUserRecord;
        }

        private ApplicationUserRecord GetUserRecordOrNull(string recordType)
        {
            if (records.ContainsKey(recordType))
                return records[recordType];
            else
                return null;
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
