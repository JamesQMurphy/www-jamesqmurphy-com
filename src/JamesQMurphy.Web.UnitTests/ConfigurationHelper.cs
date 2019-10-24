using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Web.UnitTests
{
    internal static class ConfigurationHelper
    {
        public static IConfiguration Create(IEnumerable<KeyValuePair<string, string>> configDictionary)
        {
            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDictionary);
            return builder.Build();
        }

        public static IOptions<T> OptionsFrom<T>(T options) where T:class, new()
        {
            return Options.Create(options);
        }
    }
}
