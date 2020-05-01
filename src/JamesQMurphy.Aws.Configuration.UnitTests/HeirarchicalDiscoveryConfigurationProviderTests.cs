using JamesQMurphy.Aws.Configuration;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Aws.Configuration.UnitTests
{
    public class HeirarchicalDiscoveryConfigurationProviderTests
    {
        private const string DELIMITER = "~";
        private const string CANT_SEE_KEY = "XX";
        class SampleHeirarchicalDiscoveryConfigurationProvider : HeirarchicalDiscoveryConfigurationProvider
        {
            public SampleHeirarchicalDiscoveryConfigurationProvider(string delimiter, string basePath) : base(delimiter, basePath) { }
            public string TestConvertToCustomDelimiter(string netDelimitedKey) => ConvertToCustomDelimiter(netDelimitedKey);
            public string TestConvertToNetConfigDelimiter(string customDelimitedKey) => ConvertToNetConfigDelimiter(customDelimitedKey);

            // To mimic permission restrictions, any key that ends with
            // CANT_SEE_KEY will not be searched or returned
            public readonly Dictionary<string, string> Data = new Dictionary<string, string>();

            public int TryLoadKeysUnderCounter = 0;
            protected override bool TryLoadKeysUnder(string basePath, IDictionary<string, string> keyValuePairs)
            {
                basePath = basePath ?? "";
                ++TryLoadKeysUnderCounter;
                if (basePath.EndsWith(CANT_SEE_KEY))
                {
                    return false;
                }
                foreach(var kvpair in Data.Where(kvp => kvp.Key.StartsWith(basePath)))
                {
                    keyValuePairs.Add(kvpair);
                }
                return true;
            }

            public int TryLoadKeyCounter = 0;
            protected override bool TryLoadKey(string key, out string value)
            {
                ++TryLoadKeyCounter;
                value = null;
                return key.EndsWith(CANT_SEE_KEY) ? false : Data.TryGetValue(key, out value);
            }

        }


        [TestCase("base", "base", ExpectedResult = "")]
        [TestCase("base", "base" + DELIMITER, ExpectedResult = "")]
        [TestCase("base" + DELIMITER, "base" + DELIMITER, ExpectedResult = "")]
        [TestCase("base", "base" + DELIMITER + "A", ExpectedResult = "A")]
        [TestCase("base", "base" + DELIMITER + "A" + DELIMITER + "B", ExpectedResult = "A:B")]
        [TestCase("base", "base" + DELIMITER + "A" + DELIMITER + "B" + DELIMITER + "C", ExpectedResult = "A:B:C")]
        public string NonNullConvertToNet(string basePath, string customKey)
        {
            return (new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath))
                .TestConvertToNetConfigDelimiter(customKey);
        }

        [TestCase("base", "", ExpectedResult = "base" + DELIMITER)]
        [TestCase("base" + DELIMITER, "", ExpectedResult = "base" + DELIMITER)]
        [TestCase("base", "A", ExpectedResult = "base" + DELIMITER + "A")]
        [TestCase("base", "A:B", ExpectedResult = "base" + DELIMITER + "A" + DELIMITER + "B")]
        [TestCase("base" + DELIMITER, "A:B:C", ExpectedResult = "base" + DELIMITER + "A" + DELIMITER + "B" + DELIMITER + "C")]
        public string NonNullConvertToCustom(string basePath, string netkey)
        {
            return (new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath))
                .TestConvertToCustomDelimiter(netkey);
        }

        [TestCase("", "")]
        [TestCase("A", "A")]
        [TestCase("A:B", "A" + DELIMITER + "B")]
        [TestCase("A:B:C", "A" + DELIMITER + "B" + DELIMITER + "C")]
        public void WithNullOrEmptyBasePath(string netKey, string customKey)
        {
            var nullBasePathProvider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, null);
            Assert.AreEqual(customKey, nullBasePathProvider.TestConvertToCustomDelimiter(netKey));
            Assert.AreEqual(netKey, nullBasePathProvider.TestConvertToNetConfigDelimiter(customKey));

            var emptyBasePathProvider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, string.Empty);
            Assert.AreEqual(customKey, emptyBasePathProvider.TestConvertToCustomDelimiter(netKey));
            Assert.AreEqual(netKey, emptyBasePathProvider.TestConvertToNetConfigDelimiter(customKey));
        }

        [Test]
        public void TryGetKeyCalledOnce_Success()
        {
            var key = "SOMEKEY";
            var value = "Some Value";
            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, "");
            provider.Data.Add(key, value);

            string valueReturned;
            Assert.IsTrue(provider.TryGet(key, out valueReturned));
            Assert.AreEqual(value, valueReturned);
            Assert.AreEqual(1, provider.TryLoadKeyCounter);

            // Try it again; counter should not go up
            Assert.IsTrue(provider.TryGet(key, out valueReturned));
            Assert.AreEqual(value, valueReturned);
            Assert.AreEqual(1, provider.TryLoadKeyCounter);
        }

        [Test]
        public void TryGetKeyCalledOnce_NotFound()
        {
            var key = "SOMEKEY";
            var value = "Some Value";
            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, "");
            provider.Data.Add(key, value);

            var keyAskedFor = "SOMEOTHERKEY";
            string valueReturned;
            Assert.IsFalse(provider.TryGet(keyAskedFor, out valueReturned));
            Assert.AreEqual(1, provider.TryLoadKeyCounter);

            // Try it again; counter should not go up
            Assert.IsFalse(provider.TryGet(keyAskedFor, out valueReturned));
            Assert.AreEqual(1, provider.TryLoadKeyCounter);
        }

        [Test]
        public void TryGet_NoLoad()
        {
            var basePath = $"{DELIMITER}basePath";
            var key1 = $"{basePath}{DELIMITER}key1";
            var key2 = $"{basePath}{DELIMITER}somethingelse{DELIMITER}key2";
            var key3 = $"{basePath}{DELIMITER}{CANT_SEE_KEY}{DELIMITER}key3";
            var value1 = "value1";
            var value2 = "value2";
            var value3 = "value3";

            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath);
            provider.Data.Add(key1, value1);
            provider.Data.Add(key2, value2);
            provider.Data.Add(key3, value3);

            string valueReturned = null;
            Assert.IsTrue(provider.TryGet("key1", out valueReturned));
            Assert.AreEqual(value1, valueReturned);
            Assert.IsTrue(provider.TryGet("somethingelse:key2", out valueReturned));
            Assert.AreEqual(value2, valueReturned);
            Assert.IsTrue(provider.TryGet($"{CANT_SEE_KEY}:key3", out valueReturned));
            Assert.AreEqual(value3, valueReturned);

            Assert.AreEqual(0, provider.TryLoadKeysUnderCounter);
            Assert.AreEqual(3, provider.TryLoadKeyCounter);
        }

        [Test]
        public void TryGet_WithLoad()
        {
            var basePath = $"{DELIMITER}basePath";
            var key1 = $"{basePath}{DELIMITER}key1";
            var key2 = $"{basePath}{DELIMITER}somethingelse{DELIMITER}key2";
            var key3 = $"{basePath}{DELIMITER}{CANT_SEE_KEY}{DELIMITER}key3";
            var value1 = "value1";
            var value2 = "value2";
            var value3 = "value3";

            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath);
            provider.Data.Add(key1, value1);
            provider.Data.Add(key2, value2);
            provider.Data.Add(key3, value3);

            provider.Load();

            string valueReturned = null;
            Assert.IsTrue(provider.TryGet("key1", out valueReturned));
            Assert.AreEqual(value1, valueReturned);
            Assert.IsTrue(provider.TryGet("somethingelse:key2", out valueReturned));
            Assert.AreEqual(value2, valueReturned);
            Assert.IsTrue(provider.TryGet($"{CANT_SEE_KEY}:key3", out valueReturned));
            Assert.AreEqual(value3, valueReturned);

            Assert.AreEqual(1, provider.TryLoadKeysUnderCounter);
            Assert.AreEqual(0, provider.TryLoadKeyCounter);
        }

        [Test]
        public void GetChildKeys_NoLoad()
        {
            var basePath = $"{DELIMITER}basePath";
            var key1 = $"{basePath}{DELIMITER}key1";
            var key2 = $"{basePath}{DELIMITER}somethingelse{DELIMITER}key2";
            var key3 = $"{basePath}{DELIMITER}{CANT_SEE_KEY}{DELIMITER}key3";
            var value1 = "value1";
            var value2 = "value2";
            var value3 = "value3";

            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath);
            provider.Data.Add(key1, value1);
            provider.Data.Add(key2, value2);
            provider.Data.Add(key3, value3);

            var keys = new List<string>(provider.GetChildKeys(Enumerable.Empty<string>(), "somethingelse"));
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual("somethingelse:key2", keys[0]);
            Assert.AreEqual(1, provider.TryLoadKeysUnderCounter);
            string valueReturned = null;
            Assert.IsTrue(provider.TryGet(keys[0], out valueReturned));
            Assert.AreEqual(value2, valueReturned);
            Assert.AreEqual(0, provider.TryLoadKeyCounter);
        }

        [Test]
        public void LoadingTopKeyLoadsSubKeys()
        {
            var basePath = $"{DELIMITER}basePath";
            var nestedBasePath = $"{basePath}{DELIMITER}nesting{DELIMITER}nesting2";
            var netDelimitedNestedPath = "nesting:nesting2";
            var key1 = $"{basePath}{DELIMITER}key1";
            var key2 = $"{nestedBasePath}{DELIMITER}key2";
            var value1 = "value1";
            var value2 = "value2";

            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, basePath);
            provider.Data.Add(key1, value1);
            provider.Data.Add(key2, value2);

            _ = provider.GetChildKeys(Enumerable.Empty<string>(), null);
            Assert.AreEqual(1, provider.TryLoadKeysUnderCounter);

            var keys = new List<string>(provider.GetChildKeys(Enumerable.Empty<string>(), netDelimitedNestedPath));
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual($"{netDelimitedNestedPath}:key2", keys[0]);
            Assert.AreEqual(1, provider.TryLoadKeysUnderCounter);
        }

        [Test]
        public void AddsEarlierKeys()
        {
            var earlierKeys = new List<string> { "A", "B", "nested:C" };
            var provider = new SampleHeirarchicalDiscoveryConfigurationProvider(DELIMITER, "");
            provider.Data[$"nested{DELIMITER}D"] = "value";
            var newList = new List<string>(provider.GetChildKeys(earlierKeys, null));
            Assert.AreEqual(4, newList.Count);
            Assert.IsTrue(newList.Contains("A"));
            Assert.IsTrue(newList.Contains("B"));
            Assert.IsTrue(newList.Contains("nested:C"));
            Assert.IsTrue(newList.Contains("nested:D"));
        }
    }
}