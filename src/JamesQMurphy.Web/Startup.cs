using JamesQMurphy.Auth;
using JamesQMurphy.Blog;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace JamesQMurphy.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie = new CookieBuilder()
                {
                    Name = ".JQM.PrivacyConsent.20191115",
                    Expiration = TimeSpan.FromDays(365),
                    IsEssential = true
                };
            });

            var webSiteOptions = services.ConfigurePoco<WebSiteOptions>(Configuration);
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            if (webSiteOptions.DataProtection == "AWS")
            {
                services.AddDataProtection()
                    .PersistKeysToAWSSystemsManager($"/{webSiteOptions.AppName}/DataProtection");
            }

            switch (Configuration["UserStore:Service"])
            {
                case "DynamoDb":
                    services.ConfigurePoco<JamesQMurphy.Auth.Aws.DynamoDbUserStorage.Options>(Configuration, "UserStore");
                    services.AddSingleton<IApplicationUserStorage, JamesQMurphy.Auth.Aws.DynamoDbUserStorage>();
                    break;

                default:  //InMemory
                    services.AddSingleton<IApplicationUserStorage, InMemoryApplicationUserStorage>();
                    break;
            }

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = ApplicationPasswordValidator<ApplicationUser>.PASSWORD_LENGTH;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<InMemoryRoleStore>()
                .AddPasswordValidator<ApplicationPasswordValidator<ApplicationUser>>()
                .AddSignInManager<ApplicationSignInManager<ApplicationUser>>();

            services.ConfigurePoco<WebSiteOptions>(Configuration);
            services.AddHealthChecks();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
            });


            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IMarkdownHtmlRenderer>(new DefaultMarkdownHtmlRenderer(Configuration["ImageBasePath"]));
            services.AddArticleStoreServices(Configuration);

            switch (Configuration["Email:Service"])
            {
                case "SES":
                    services.ConfigurePoco<JamesQMurphy.Email.Aws.SESEmailService.Options>(Configuration, "Email");
                    services.AddSingleton<IEmailService, JamesQMurphy.Email.Aws.SESEmailService>();
                    break;

                case "SQS":
                    services.ConfigurePoco<JamesQMurphy.Email.Aws.SQSEmailService.Options>(Configuration, "Email");
                    services.AddSingleton<IEmailService, JamesQMurphy.Email.Aws.SQSEmailService>();
                    break;

                default: //NullEmailService
                    services.AddSingleton<IEmailService, NullEmailService>();
                    break;
            }
            services.AddTransient<IEmailGenerator, BasicEmailGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHealthChecks(Configuration["WarmUrl"]);
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // If using a LocalFolder article store, map ImageBasePath to the article store path so that the images load
            if ((Configuration["ArticleStore:Service"] == "LocalFolder") && (Configuration["ImageBasePath"] != "/"))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.GetFullPath(Configuration["ArticleStore:Path"])),
                    RequestPath = Configuration["ImageBasePath"]
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "blogIndex",
                    template: "blog/{year?}/{month?}",
                    defaults: new { controller = "Blog", action = "Index" });

                routes.MapRoute(
                    name: "blogDetails",
                    template: "blog/{year}/{month}/{slug}",
                    defaults: new { controller = "Blog", action = "Details" });

                routes.MapRoute(
                    name: "blogDetails",
                    template: "blog/{year}/{month}/{slug}/comments",
                    defaults: new { controller = "Blog", action = "Comments" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
