using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace JamesQMurphy.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // AWS Lambda will call the "bootstrap" shell script, which
            // will pass the string "lambda" as an argument
            if (args.Length > 0 && args[0] == "lambda")
            {
                var lambdaEntryPoint = new LambdaEntryPoint();

                var handlerWrapper = HandlerWrapper.GetHandlerWrapper<APIGatewayProxyRequest, APIGatewayProxyResponse>(lambdaEntryPoint.FunctionHandlerAsync, new JsonSerializer());
                using (handlerWrapper)
                {
                    using (var lambdaBootstrap = new LambdaBootstrap(handlerWrapper))
                    {
                        lambdaBootstrap.RunAsync().GetAwaiter().GetResult();
                    }
                }
            }
            // Otherwise, it gets called like most ASP.NET Core web apps
            else
            {
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
