using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JamesQMurphy.Aws.Configuration
{
    public abstract class HeirarchicalDiscoveryConfigurationProvider : IConfigurationProvider
    {
        public string Delimiter { get; }
        public string BasePath { get; }
        private readonly string _basePathWithDelimiter;
        private Dictionary<string, string> _data = new Dictionary<string, string>();
        private HashSet<string> _keysNotFound = new HashSet<string>();
        private HashSet<string> _pathsSearched = new HashSet<string>();
        public HeirarchicalDiscoveryConfigurationProvider(string delimiter, string basePath)
        {
            Delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
            BasePath = basePath ?? "";
            _basePathWithDelimiter = BasePath.Length==0 || BasePath.EndsWith(Delimiter) ? BasePath : BasePath + Delimiter;
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            // Fix parent path so that it is not null and doesn't end with a delimiter
            parentPath = parentPath ?? "";
            if (parentPath.EndsWith(ConfigurationPath.KeyDelimiter))
            {
                parentPath = parentPath.Substring(0, parentPath.Length - ConfigurationPath.KeyDelimiter.Length);
            }

            var valuesReturned = new Dictionary<string, string>();
            if (!_pathsSearched.Contains(parentPath))
            {
                _pathsSearched.Add(parentPath);
                if (TryLoadKeysUnder(ConvertToCustomDelimiter(parentPath), valuesReturned))
                {
                    foreach (var kvp in valuesReturned)
                    {
                        var netkey = ConvertToNetConfigDelimiter(kvp.Key);
                        _data[netkey] = kvp.Value;

                        // Mark the child path as searched
                        var indexOfLastDelimiter = netkey.LastIndexOf(ConfigurationPath.KeyDelimiter);
                        if (indexOfLastDelimiter > 0)
                        {
                            _pathsSearched.Add(netkey.Substring(0, indexOfLastDelimiter));
                        }
                    }
                }
            }
            return _data.Keys
                .Where(k => k.StartsWith(parentPath))
                .Concat(earlierKeys)
                .Distinct();
        }

        private IChangeToken _reloadToken = new ConfigurationReloadToken();
        public IChangeToken GetReloadToken() => _reloadToken;

        public void Load()
        {
            _ = GetChildKeys(Enumerable.Empty<string>(), null);
        }

        public void Set(string key, string value) { }

        public bool TryGet(string key, out string value)
        {
            value = null;
            if (_data.TryGetValue(key, out value))
            {
                return true;
            }
            if (_keysNotFound.Contains(key))
            {
                return false;
            }

            if (TryLoadKey(ConvertToCustomDelimiter(key), out value))
            {
                _data[key] = value;
                return true;
            }
            else
            {
                _keysNotFound.Add(key);
                return false;
            }
        }

        protected abstract bool TryLoadKeysUnder(string basePath, IDictionary<string, string> keyValuePairs);
        protected abstract bool TryLoadKey(string key, out string value);

        protected virtual string ConvertToNetConfigDelimiter(string customDelimitedKey)
        {
            var intermediate = customDelimitedKey.Substring(BasePath.Length).Replace(Delimiter, ConfigurationPath.KeyDelimiter);
            return intermediate.StartsWith(ConfigurationPath.KeyDelimiter) ? intermediate.Substring(ConfigurationPath.KeyDelimiter.Length) : intermediate;
        }
        protected virtual string ConvertToCustomDelimiter(string netDelimitedKey)
        {
            return _basePathWithDelimiter + (netDelimitedKey ?? "").Replace(ConfigurationPath.KeyDelimiter, Delimiter);
        }
    }
}
