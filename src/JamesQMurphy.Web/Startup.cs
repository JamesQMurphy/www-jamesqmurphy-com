using Amazon.SimpleSystemsManagement;
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
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

namespace JamesQMurphy.Web
{
    public class Startup
    {
        private const string AUTH_TWITTER_CLIENT_ID = "Authentication:Twitter:ConsumerAPIKey";
        private const string AUTH_TWITTER_CLIENT_SECRET = "Authentication:Twitter:ConsumerSecret";
        private const string AUTH_GITHUB_CLIENT_ID = "Authentication:GitHub:ClientId";
        private const string AUTH_GITHUB_CLIENT_SECRET = "Authentication:GitHub:ClientSecret";
        private const string AUTH_GOOGLE_CLIENT_ID = "Authentication:Google:ClientId";
        private const string AUTH_GOOGLE_CLIENT_SECRET = "Authentication:Google:ClientSecret";

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

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
                    Name = ".JQM.PrivacyConsent.20200203",
                    Expiration = TimeSpan.FromDays(365),
                    IsEssential = true
                };
            });

            // The Tempdata provider cookie is not essential. Make it essential
            // so Tempdata is functional when tracking is disabled.
            // See https://stackoverflow.com/a/54813987/1001100
            services.Configure<CookieTempDataProviderOptions>(options => {
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".JQM.ResultMessages";
            });

            var webSiteOptions = services.ConfigurePoco<WebSiteOptions>(Configuration, "WebSiteOptions");
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            if (webSiteOptions.DataProtection == "AWS")
            {
                services.AddDataProtection()
                    .PersistKeysToAWSSystemsManager($"/{System.Environment.GetEnvironmentVariable("ApplicationStageKey")}/DataProtection");
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
            services.AddSingleton<ApplicationUserConfirmation>();
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<InMemoryRoleStore>()
                .AddPasswordValidator<ApplicationPasswordValidator<ApplicationUser>>()
                .AddSignInManager<ApplicationSignInManager<ApplicationUser>>();

            var authBuilder = services.AddAuthentication();
            if (!String.IsNullOrWhiteSpace(Configuration[AUTH_TWITTER_CLIENT_ID]))
            {
                authBuilder.AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration[AUTH_TWITTER_CLIENT_ID];
                    options.ConsumerSecret = Configuration[AUTH_TWITTER_CLIENT_SECRET];
                    options.CallbackPath = "/account/login-twitter";
                });
            }
            if (!String.IsNullOrWhiteSpace(Configuration[AUTH_GITHUB_CLIENT_ID]))
            {
                authBuilder.AddGitHub(options =>
                {
                    options.ClientId = Configuration[AUTH_GITHUB_CLIENT_ID];
                    options.ClientSecret = Configuration[AUTH_GITHUB_CLIENT_SECRET];
                    options.CallbackPath = "/account/login-github";
                });
            }
            if (!String.IsNullOrWhiteSpace(Configuration[AUTH_GOOGLE_CLIENT_ID]))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = Configuration[AUTH_GOOGLE_CLIENT_ID];
                    options.ClientSecret = Configuration[AUTH_GOOGLE_CLIENT_SECRET];
                    options.CallbackPath = "/account/login-google";
                });
            }

            services.AddHealthChecks();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/accessdenied";
            });
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".JQM.AntiForgery";
            });

            // The "new" way to do AddMvc()
            services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            var mvcBuilder = services.AddRazorPages();
#if DEBUG
            // This lets you edit razor files while the app is running
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.AddTransient<IMarkdownHtmlRenderer, WebsiteMarkupRenderer>();
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

                case "Mailgun":
                    services.ConfigurePoco<JamesQMurphy.Email.Mailgun.MailgunEmailService.Options>(Configuration, "Email");
                    services.AddSingleton<IEmailService, JamesQMurphy.Email.Mailgun.MailgunEmailService>();
                    break;

                default: //NullEmailService
                    services.AddSingleton<IEmailService, NullEmailService>();
                    break;
            }
            services.AddTransient<IEmailGenerator, BasicEmailGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/home/error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (Configuration["UseStaticFiles"].ToLowerInvariant() == "true")
            {
                app.UseStaticFiles();
            }
            // If using a LocalFolder article store, map ImageBasePath to the article store path so that the images load
            if ((Configuration["ArticleStore:Service"] == "LocalFolder") && (Configuration["ImageBasePath"] != "/"))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.GetFullPath(Configuration["ArticleStore:Path"])),
                    RequestPath = Configuration["ImageBasePath"]
                });
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(Configuration["WarmUrl"]);

                endpoints.MapControllerRoute(
                    name: "blogIndex",
                    pattern: "blog/{year?}/{month?}",
                    defaults: new { controller = "blog", action = "index" });

                endpoints.MapControllerRoute(
                    name: "blogDetails",
                    pattern: "blog/{year}/{month}/{slug}",
                    defaults: new { controller = "blog", action = "details" });

                endpoints.MapControllerRoute(
                    name: "blogDetailsComments",
                    pattern: "blog/{year}/{month}/{slug}/comments",
                    defaults: new { controller = "blog", action = "comments" });

                endpoints.MapControllerRoute(
                    name: "profile",
                    pattern: "profile/{username}",
                    defaults: new { controller = "profile", action = "index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=home}/{action=index}/{id?}");
            });
        }
    }
}
