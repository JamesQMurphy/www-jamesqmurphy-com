using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace JamesQMurphy.Auth
{
    public class ApplicationUserRecord
    {
        public const string RECORD_TYPE_ID = "ID";
        public const string RECORD_TYPE_EMAILPROVIDER = "Email";

        public string Provider { get; }
        public string Key { get; }
        public string UserId { get; }

        private string _normalizedKey;
        public string NormalizedKey
        {
            get => _normalizedKey;
            set
            {
                if (_normalizedKey != value)
                {
                    _normalizedKey = value;
                    SetDirty();
                }
            }
        }

        public DateTime LastUpdated { get; private set; }
        public bool IsDirty => LastUpdated == DateTime.MinValue;
        private void SetDirty() => LastUpdated = DateTime.MinValue;

        private readonly ImmutableDictionary<string, string>.Builder _stringDictBuilder;
        private readonly ImmutableDictionary<string, bool>.Builder _boolDictBuilder;
        public ImmutableDictionary<string, string> StringAttributes => _stringDictBuilder.ToImmutable();
        public ImmutableDictionary<string, bool> BoolAttributes => _boolDictBuilder.ToImmutable();

        public ApplicationUserRecord(string provider, string key, string userId)
        {
            Provider = provider;
            Key = key;
            UserId = userId;
            _normalizedKey = key;
            LastUpdated = DateTime.MinValue;
            _stringDictBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            _boolDictBuilder = ImmutableDictionary.CreateBuilder<string, bool>();
        }

        public ApplicationUserRecord(string provider, string key, string userId, string normalizedKey, DateTime lastUpdated) :
            this(provider, key, userId)
        {
            _normalizedKey = normalizedKey;
            LastUpdated = lastUpdated;
        }

        public ApplicationUserRecord(string provider, string key, string userId, string normalizedKey, DateTime lastUpdated, IDictionary<string,string> stringAttributes, IDictionary<string,bool> boolAttributes) :
            this(provider, key, userId, normalizedKey, lastUpdated)
        {
            if (stringAttributes != null)
            {
                foreach (var kvp in stringAttributes)
                {
                    _stringDictBuilder.Add(kvp);
                }
            }
            if (boolAttributes != null)
            {
                foreach (var kvp in boolAttributes)
                {
                    _boolDictBuilder.Add(kvp);
                }
            }
        }

        public void SetStringAttribute(string attribute, string value)
        {
            if (_stringDictBuilder.TryGetValue(attribute, out string currentValue))
            {
                if (value == currentValue)
                {
                    return;
                }
            }
            SetDirty();
            _stringDictBuilder[attribute] = value;
        }
        public void SetBoolAttribute(string attribute, bool value)
        {
            if (_boolDictBuilder.TryGetValue(attribute, out bool currentValue))
            {
                if (value == currentValue)
                {
                    return;
                }
            }
            SetDirty();
            _boolDictBuilder[attribute] = value;
        }

        public static ApplicationUserRecord CreateCleanRecord(ApplicationUserRecord applicationUserRecord, DateTime lastUpdated)
        {
            return new ApplicationUserRecord(
                applicationUserRecord.Provider,
                applicationUserRecord.Key,
                applicationUserRecord.UserId,
                applicationUserRecord.NormalizedKey,
                lastUpdated,
                applicationUserRecord.StringAttributes,
                applicationUserRecord.BoolAttributes
            );
        }
    }
}
