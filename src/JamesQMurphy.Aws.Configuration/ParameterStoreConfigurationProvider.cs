using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Aws.Configuration
{
    public class ParameterStoreConfigurationProvider : HeirarchicalDiscoveryConfigurationProvider
    {
        public ParameterStoreConfigurationSource ParameterStoreConfigurationSource { get; }
        public ParameterStoreConfigurationProvider(ParameterStoreConfigurationSource parameterStoreConfigurationSource)
            : base("/", parameterStoreConfigurationSource.BasePath)
        {
            ParameterStoreConfigurationSource = parameterStoreConfigurationSource;
        }

        protected override bool TryLoadKeysUnder(string basePath, IDictionary<string, string> keyValuePairs)
        {
            try
            {
                string nextToken = default;
                do
                {
                    var response = ParameterStoreConfigurationSource.AmazonSimpleSystemsManagementClient.GetParametersByPathAsync(
                        new GetParametersByPathRequest
                        {
                            Path = basePath,
                            WithDecryption = true,
                            Recursive = true,
                            NextToken = nextToken
                        }).GetAwaiter().GetResult();

                    // Store the keys/values that we got back
                    foreach (var parameter in response.Parameters)
                    {
                        keyValuePairs[parameter.Name] = parameter.Value;
                    }

                    nextToken = response.NextToken;
                } while (!String.IsNullOrEmpty(nextToken));
            }
            catch (Amazon.Runtime.AmazonServiceException)
            {
                // Typically an IAM permissions issue, but whatever the issue,
                // we cannot search the path
                return false;
            }
            return true;
        }

        protected override bool TryLoadKey(string key, out string value)
        {
            try
            {
                var response = ParameterStoreConfigurationSource.AmazonSimpleSystemsManagementClient.GetParameterAsync(
                    new GetParameterRequest
                    {
                        Name = key,
                        WithDecryption = true
                    }).GetAwaiter().GetResult();
                value = response.Parameter.Value;
                return true;
            }
            catch (Amazon.Runtime.AmazonServiceException)
            {
                value = null;
                return false;
            }
        }
    }
}
