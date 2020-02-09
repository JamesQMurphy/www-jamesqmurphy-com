using JamesQMurphy.Auth;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;


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

            serviceCollection.AddSingleton<ILogger<accountController>>(NullLogger<accountController>.Instance);
            serviceCollection.AddSingleton<ILogger<adminController>>(NullLogger<adminController>.Instance);
            serviceCollection.AddSingleton<ILogger<blogController>>(NullLogger<blogController>.Instance);
            serviceCollection.AddSingleton<ILogger<homeController>>(NullLogger<homeController>.Instance);

            serviceCollection.AddSingleton<ILogger<UserManager<ApplicationUser>>>(NullLogger<UserManager<ApplicationUser>>.Instance);
            serviceCollection.AddSingleton<ILogger<RoleManager<ApplicationRole>>>(NullLogger<RoleManager<ApplicationRole>>.Instance);
            serviceCollection.AddSingleton<ILogger<SignInManager<ApplicationUser>>>(NullLogger<SignInManager<ApplicationUser>>.Instance);
            serviceCollection.AddSingleton<ILogger<DataProtectorTokenProvider<ApplicationUser>>>(NullLogger<DataProtectorTokenProvider<ApplicationUser>>.Instance);

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

            serviceCollection.AddSingleton<ITempDataDictionaryFactory>(new TempDataDictionaryFactory(new NullTempDataProvider()));

            var serviceProvider = serviceCollection.BuildServiceProvider();

            httpContextAccessor.HttpContext.RequestServices = serviceProvider;

            return serviceProvider;
        }

        public static ClaimsPrincipal ToClaimsPrincipal(this ApplicationUser user)
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
