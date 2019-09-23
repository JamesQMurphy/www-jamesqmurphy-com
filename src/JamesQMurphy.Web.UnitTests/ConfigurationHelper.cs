using System;
using System.Collections.Generic;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AccountViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;


namespace JamesQMurphy.Web.UnitTests
{
    internal static class ConfigurationHelper
    {
        public static IConfiguration Create(IEnumerable<KeyValuePair<string, string>> configDictionary)
        {
            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDictionary);
            return builder.Build();
        }

        public static IOptions<T> OptionsFrom<T>(T options) where T:class, new()
        {
            return Options.Create(options);
        }

        public static ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ILogger<AccountController>>(NullLogger<AccountController>.Instance);
            serviceCollection.AddSingleton<ILogger<AdminController>>(NullLogger<AdminController>.Instance);
            serviceCollection.AddSingleton<ILogger<BlogController>>(NullLogger<BlogController>.Instance);
            serviceCollection.AddSingleton<ILogger<HomeController>>(NullLogger<HomeController>.Instance);

            serviceCollection.AddSingleton<ILogger<UserManager<ApplicationUser>>>(NullLogger<UserManager<ApplicationUser>>.Instance);
            serviceCollection.AddSingleton<ILogger<RoleManager<ApplicationRole>>>(NullLogger<RoleManager<ApplicationRole>>.Instance);
            serviceCollection.AddSingleton<ILogger<SignInManager<ApplicationUser>>>(NullLogger<SignInManager<ApplicationUser>>.Instance);

            serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<InMemoryRoleStore>()
                .AddPasswordValidator<ApplicationPasswordValidator<ApplicationUser>>()
                .AddSignInManager<ApplicationSignInManager<ApplicationUser>>();

            serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            serviceCollection.AddSingleton<IApplicationUserStorage, InMemoryApplicationUserStorage>();
            serviceCollection.AddSingleton<IEmailService, NullEmailService>();
            serviceCollection.AddSingleton<IEmailGenerator, MockEmailGenerator>();

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            serviceCollection.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            httpContextAccessor.HttpContext.RequestServices = serviceProvider;

            return serviceProvider;
        }
    }
}
