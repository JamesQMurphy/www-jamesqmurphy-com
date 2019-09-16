﻿using System;
using System.Collections.Generic;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ProfileViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Identity;
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
            serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<InMemoryRoleStore>()
                .AddSignInManager<ApplicationSignInManager<ApplicationUser>>();
            serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            serviceCollection.AddSingleton<ILogger<UserManager<ApplicationUser>>>(NullLogger<UserManager<ApplicationUser>>.Instance);
            serviceCollection.AddSingleton<ILogger<RoleManager<ApplicationRole>>>(NullLogger<RoleManager<ApplicationRole>>.Instance);
            serviceCollection.AddSingleton<ILogger<SignInManager<ApplicationUser>>>(NullLogger<SignInManager<ApplicationUser>>.Instance);
            serviceCollection.AddSingleton<ILogger<ProfileController>>(NullLogger<ProfileController>.Instance);
            serviceCollection.AddSingleton<IApplicationUserStorage, InMemoryApplicationUserStorage>();
            serviceCollection.AddSingleton<IEmailService, NullEmailService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.RequestServices = serviceProvider;

            return serviceProvider;
        }
    }
}
