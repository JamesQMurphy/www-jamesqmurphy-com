using Amazon.SimpleSystemsManagement;
using JamesQMurphy.Aws.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public static class Extensions
    {
        public static IConfigurationBuilder AddSmsParameterStore(
            this IConfigurationBuilder configuration,
            string BasePath = "/",
            AmazonSimpleSystemsManagementClient client = null)
        {
            configuration.Add(new ParameterStoreConfigurationSource(BasePath, client ?? new AmazonSimpleSystemsManagementClient()));
            return configuration;
        }
    }
}
