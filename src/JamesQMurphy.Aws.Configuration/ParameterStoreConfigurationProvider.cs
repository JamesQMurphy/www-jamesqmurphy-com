using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Aws.Configuration
{
    public class ParameterStoreConfigurationProvider : ConfigurationProvider
    {
        public static readonly string KeyDelimiter = "/";

        public ParameterStoreConfigurationSource ParameterStoreConfigurationSource { get; }
        public ParameterStoreConfigurationProvider(ParameterStoreConfigurationSource parameterStoreConfigurationSource)
        {
            ParameterStoreConfigurationSource = parameterStoreConfigurationSource;
        }

        public override void Load()
        {
            var basePathLength = ParameterStoreConfigurationSource.BasePath.Length;
            try
            {
                string nextToken = default;
                do
                {
                    // Query AWS Parameter Store
                    var response = ParameterStoreConfigurationSource.AmazonSimpleSystemsManagementClient.GetParametersByPathAsync(
                        new GetParametersByPathRequest
                        {
                            Path = ParameterStoreConfigurationSource.BasePath,
                            WithDecryption = true,
                            Recursive = true,
                            NextToken = nextToken
                        }).GetAwaiter().GetResult();

                    // Store the keys/values that we got back
                    foreach (var parameter in response.Parameters)
                    {
                        var dotNetKey = parameter.Name.Substring(basePathLength).Replace(KeyDelimiter, ConfigurationPath.KeyDelimiter);
                        Data[dotNetKey] = parameter.Value;
                    }

                    // Possibly get more
                    nextToken = response.NextToken;

                } while (!String.IsNullOrEmpty(nextToken));
            }
            catch (Amazon.Runtime.AmazonServiceException)
            {
                // Typically an IAM permissions issue, but could also be that
                // there are no values to retrieve.
                return;
            }
        }
    }

    public class ParameterStoreConfigurationProvider_Old : HeirarchicalDiscoveryConfigurationProvider
    {
        public ParameterStoreConfigurationSource ParameterStoreConfigurationSource { get; }
        public ParameterStoreConfigurationProvider_Old(ParameterStoreConfigurationSource parameterStoreConfigurationSource)
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
