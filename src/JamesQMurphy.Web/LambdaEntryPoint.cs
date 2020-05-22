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
                .UseStartup<StartupAws>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.Sources.Clear();

                    // Configuration sources for running under AWS Lambda
                    config
                        .AddJsonFile("appsettings.json", optional: false)

                        .AddJsonFile("appsettings.aws.json", optional: false)

                        // appsettings.{EnvironmentName}.json intentionally omitted

                        // We can't use builderContext.ApplicationName thanks to this issue:
                        // https://github.com/dotnet/aspnetcore/issues/11085
                        .AddSsmParameterStore($"/AppSettings/{System.Environment.GetEnvironmentVariable("ApplicationStageKey")}")

                        .AddEnvironmentVariables();

                        // Command-line configuration provider intentionally omitted
                });
        }
    }
}