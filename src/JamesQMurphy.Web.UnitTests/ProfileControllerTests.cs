using System;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ProfileViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace JamesQMurphy.Web.UnitTests
{
    public class ProfileControllerTests
    {
        private ProfileController _controller;
        private ServiceProvider _serviceProvider; 
        private Microsoft.AspNetCore.Http.HttpContextAccessor _httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();

        [SetUp]
        public void Setup()
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
            _serviceProvider = serviceCollection.BuildServiceProvider();

            _httpContextAccessor.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            _httpContextAccessor.HttpContext.RequestServices = _serviceProvider;

            _controller = new ProfileController(
                _serviceProvider.GetService<ApplicationSignInManager<ApplicationUser>>(),
                _serviceProvider.GetService<ILogger<ProfileController>>(),
                _serviceProvider.GetService<IEmailService>());
        }

        [Test]
        public void TestLogin()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";

            var user = AddExistingUser(_serviceProvider, email, password, username);
            var loginViewModel = new LoginViewModel()
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            var result = _controller.Login(loginViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.RedirectToActionResult>(result);
            var redirectToActionResult = result as Microsoft.AspNetCore.Mvc.RedirectToActionResult;
            Assert.AreEqual("Home", redirectToActionResult.ControllerName);
            Assert.AreEqual(nameof(HomeController.Index), redirectToActionResult.ActionName);
        }

        private static ApplicationUser AddExistingUser(IServiceProvider serviceProvider, string emailAddress, string password, string userName)
        {
            var normalizer = serviceProvider.GetService<ILookupNormalizer>();
            var user = new ApplicationUser()
            {
                Email = emailAddress,
                NormalizedEmail = normalizer.Normalize(emailAddress),
                UserName = userName,
                NormalizedUserName = normalizer.Normalize(userName)
            };
            user.PasswordHash = serviceProvider.GetService<IPasswordHasher<ApplicationUser>>().HashPassword(user, password);
            var storage = serviceProvider.GetService<IApplicationUserStorage>();
            storage.CreateAsync(user).GetAwaiter().GetResult();
            return user;
        }

    }
}