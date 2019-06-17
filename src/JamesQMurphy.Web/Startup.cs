using System;
using System.Collections.Generic;
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

            var awsOptions = Configuration.GetAWSOptions();
            if (awsOptions.Region != null)
            {
                services.AddDefaultAWSOptions(awsOptions);
                services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
                services.AddSingleton<IArticleStore, JamesQMurphy.Blog.Aws.DynamoDbArticleStore>();
            }
            else
            {
                var articleStore = new InMemoryArticleStore();
                articleStore.Articles.AddRange(new Article[]
                {
                new Article()
                {
                    Title = "Article One",
                    Slug = "article-1",
                    PublishDate = new DateTime(2019, 1, 10, 12, 34, 56),
                    Content = "This is article one, published on January 10, 2019 at 12:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Two",
                    Slug = "article-2",
                    PublishDate = new DateTime(2019, 7, 6, 18, 34, 56),
                    Content = "This is article two, published on July 6, 2019 at 6:34pm UTC"
                },

                new Article()
                {
                    Title = "Article Three",
                    Slug = "article-3",
                    PublishDate = new DateTime(2019, 8, 31, 10, 2, 0),
                    Content = "This is article three, published on August 31, 2019 at 10:02am UTC"
                }
                });
                services.AddSingleton<IArticleStore>(articleStore);
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
