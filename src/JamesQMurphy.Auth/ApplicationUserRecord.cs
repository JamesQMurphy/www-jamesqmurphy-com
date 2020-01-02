using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Auth
{
    public class ApplicationUserRecord
    {
        public const string RECORD_TYPE_ID = "ID";
        public const string RECORD_TYPE_EMAIL = "ByEmail";
        public const string RECORD_TYPE_USERNAME = "ByUsername";

        public string Provider { get; }
        public string Key { get; }
        public string UserId { get; }

        private string _normalizedKey;
        public string NormalizedKey
        {
            get => _normalizedKey;
            set
            {
                _normalizedKey = value;
                SetDirty();
            }
        }

        public DateTime LastUpdated { get; private set; }
        public bool IsDirty => LastUpdated == DateTime.MinValue;
        private void SetDirty() => LastUpdated = DateTime.MinValue;
        public readonly Dictionary<string, string> StringAttributes = new Dictionary<string, string>();
        public readonly Dictionary<string, bool> BoolAttributes = new Dictionary<string, bool>();

        public ApplicationUserRecord(string provider, string key, string userId)
        {
            Provider = provider;
            Key = key;
            _normalizedKey = "";
            UserId = userId;
            LastUpdated = DateTime.MinValue;
        }

        public ApplicationUserRecord(string provider, string key, string normalizedKey, string userId, DateTime lastUpdated)
        {
            Provider = provider;
            Key = key;
            _normalizedKey = normalizedKey;
            UserId = userId;
            LastUpdated = lastUpdated;
        }

    }
}
