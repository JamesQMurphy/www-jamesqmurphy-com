using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JamesQMurphy.Web
{
    public class StartupAws
    {
        public StartupAws(IConfiguration configuration)
        {
            Configuration = configuration;
            Startup = new Startup(configuration);
        }

        public IConfiguration Configuration { get; }
        public Startup Startup { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();

            Startup.ConfigureServices(services);

            services.AddDataProtection()
                .PersistKeysToAWSSystemsManager($"/{Environment.GetEnvironmentVariable("ApplicationStageKey")}/DataProtection");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) => Startup.Configure(app, env);
    }
}
