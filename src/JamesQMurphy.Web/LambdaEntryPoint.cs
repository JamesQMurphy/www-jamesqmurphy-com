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
                    config.Sources.Clear();

                    // Configuration sources for running under AWS Lambda
                    config
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddSsmParameterStore($"/{System.Environment.GetEnvironmentVariable("AppName")}")
                        .AddEnvironmentVariables();
                    // We are intentionally omitting the command-line configuration provider
                });
        }
    }
}