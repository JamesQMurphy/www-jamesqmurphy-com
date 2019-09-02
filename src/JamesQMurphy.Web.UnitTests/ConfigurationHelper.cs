using Microsoft.Extensions.Configuration;
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
    }
}
