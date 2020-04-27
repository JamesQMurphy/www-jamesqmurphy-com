using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Aws.Configuration
{
    public class ParameterStoreConfigurationProvider : IConfigurationProvider
    {
        public ParameterStoreConfigurationSource ParameterStoreConfigurationSource { get; }
        public ParameterStoreConfigurationProvider(ParameterStoreConfigurationSource parameterStoreConfigurationSource)
        {
            ParameterStoreConfigurationSource = parameterStoreConfigurationSource;
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var listEarlierKeys = new List<string>(earlierKeys);

            try
            {
                string nextToken = null;
                do
                {
                    var response = ParameterStoreConfigurationSource.AmazonSimpleSystemsManagementClient.GetParametersByPathAsync(
                        new GetParametersByPathRequest
                        {
                            Path = $"{ParameterStoreConfigurationSource.BasePath}{ConvertToSMSHeirarchy(parentPath)}",
                            WithDecryption = true,
                            NextToken = nextToken
                        }).GetAwaiter().GetResult();
                    var keysToAdd = response.Parameters
                        .Select(p => p.Name)
                        .Where(k => !listEarlierKeys.Contains(k));
                    listEarlierKeys.AddRange(keysToAdd);
                    nextToken = response.NextToken;
                } while (!String.IsNullOrEmpty(nextToken));

                return listEarlierKeys;
            }
            catch (Amazon.Runtime.AmazonServiceException)
            {
                return earlierKeys;
            }
        }

        public IChangeToken GetReloadToken() => null;

        public void Load() {}
        public void Set(string key, string value) { }

        public bool TryGet(string key, out string value)
        {
            try
            {

                var response = ParameterStoreConfigurationSource.AmazonSimpleSystemsManagementClient.GetParameterAsync(
                    new GetParameterRequest
                    {
                        Name = $"{ParameterStoreConfigurationSource.BasePath}{ConvertToSMSHeirarchy(key)}",
                        WithDecryption = true
                    }).GetAwaiter().GetResult();
                value = response.Parameter.Value;
                return true;
            }
            catch(Amazon.Runtime.AmazonServiceException)
            {
                value = null;
                return false;
            }
        }

        private static string ConvertToSMSHeirarchy(string parameterName) => parameterName.Replace(':', '/');
    }
}
