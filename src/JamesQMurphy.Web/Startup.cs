using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JamesQMurphy.Blog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IMarkdownHtmlRenderer>(new DefaultMarkdownHtmlRenderer(Configuration["ImageBasePath"]));

            switch (Configuration["ArticleStore:Service"])
            {
                case "LocalFolder":
                    services.AddSingleton<IArticleStore>(new LocalFolderArticleStore(Configuration["ArticleStore:Path"]));
                    break;

                case "Lambda":
                    services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                    services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
                    services.AddSingleton<IArticleStore, JamesQMurphy.Blog.Aws.DynamoDbArticleStore>();
                    break;

                default:  // InMemoryArticleStore
                    services.AddSingleton<IArticleStore, InMemoryArticleStore>();
                    break;
            }
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
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
