using JamesQMurphy.Email;
using JamesQMurphy.Email.Mailgun;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Messaging.Aws
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEmailService>(
                new MailgunEmailService(
                    new MailgunEmailService.Options
                    {
                        FromAddress = Configuration["Email__FromAddress"],
                        MailDomain = Configuration["Email__MailDomain"],
                        ServiceApiKey = Configuration["Email__ServiceApiKey"],  //TODO - get from SMS
                        ServiceUrl = Configuration["Email__ServiceUrl"]
                    }
                )
            );
        }

    }
}
