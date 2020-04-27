using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Aws.Configuration
{
    public class ParameterStoreConfigurationSource : IConfigurationSource
    {
        public string BasePath { get; }
        public AmazonSimpleSystemsManagementClient AmazonSimpleSystemsManagementClient { get; }
        public ParameterStoreConfigurationSource(string basePath, AmazonSimpleSystemsManagementClient amazonSimpleSystemsManagementClient)
        {
            if (basePath.EndsWith("/"))
            {
                BasePath = basePath;
            }
            else
            {
                BasePath = $"{basePath}/";
            }
            AmazonSimpleSystemsManagementClient = amazonSimpleSystemsManagementClient;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ParameterStoreConfigurationProvider(this);
        }
    }
}
