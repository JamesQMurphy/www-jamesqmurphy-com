using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JamesQMurphy.Web
{
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    // Spin up a WebSiteOptions here from the existing configuration so that
                    // we can get the AppName, which will be the root of the SMS Parameter Store config
                    var webSiteOptions = new WebSiteOptions();
                    builderContext.Configuration.Bind(webSiteOptions);

                    // Now add the Parameter Store as a config source
                    config.AddSmsParameterStore($"/{webSiteOptions.AppName}");
                });
        }
    }
}