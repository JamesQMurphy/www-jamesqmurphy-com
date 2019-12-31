using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Auth
{
    public class ApplicationUserRecord
    {
        public string Provider { get; set; }
        public string Key { get; set; }
        public string NormalizedKey { get; set; }
        public string UserId { get; set; }
        public DateTime LastUpdated { get; set; }
        public readonly Dictionary<string, string> StringAttributes = new Dictionary<string, string>();
        public readonly Dictionary<string, bool> BoolAttributes = new Dictionary<string, bool>();

        public ApplicationUserRecord(string provider, string key, string normalizedKey, string userId)
        {
            Provider = provider;
            Key = key;
            NormalizedKey = normalizedKey;
            UserId = userId;
        }
    }
}
