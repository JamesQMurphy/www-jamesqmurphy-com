using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ProfileViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace JamesQMurphy.Web.UnitTests
{
    public class ProfileControllerTests
    {
        private ProfileController controller;
        private ApplicationUserStore _userStore;
        private ServiceCollection _serviceCollection = new ServiceCollection();
        private PasswordHasher<ApplicationUser> _pwHasher = new PasswordHasher<ApplicationUser>();
        private UpperInvariantLookupNormalizer _normalizer = new UpperInvariantLookupNormalizer();
        private Microsoft.AspNetCore.Http.HttpContextAccessor _httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();

        [SetUp]
        public void Setup()
        {
            _serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<InMemoryRoleStore>()
                .AddSignInManager<ApplicationSignInManager<ApplicationUser>>();
            _serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            _httpContextAccessor.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            _httpContextAccessor.HttpContext.RequestServices = serviceProvider;

            _userStore = new ApplicationUserStore(new InMemoryApplicationUserStorage());

            var identityOptions = new IdentityOptions();
            var identityOptionsAccessor = ConfigurationHelper.OptionsFrom(identityOptions);

            var userManager = new UserManager<ApplicationUser>(
                _userStore,
                identityOptionsAccessor,
                _pwHasher,
                null,
                null,
                _normalizer,
                null,
                serviceProvider,
                NullLogger<UserManager<ApplicationUser>>.Instance);

            var roleManager = new RoleManager<ApplicationRole>(
                new InMemoryRoleStore(),
                null,
                _normalizer,
                null,
                NullLogger<RoleManager<ApplicationRole>>.Instance);

            var userClaimsPrincipalFactory = new UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(
                userManager,
                roleManager,
                identityOptionsAccessor);

            var signInManager = new ApplicationSignInManager<ApplicationUser>(
                userManager,
                _httpContextAccessor,
                userClaimsPrincipalFactory,
                identityOptionsAccessor,
                NullLogger<SignInManager<ApplicationUser>>.Instance,
                null);

            controller = new ProfileController(signInManager, NullLogger<ProfileController>.Instance, new NullEmailService());
        }

        [Test]
        public void TestLogin()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";
            var user = AddExistingUser(email, password, username);

            var loginViewModel = new LoginViewModel()
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            var result = controller.Login(loginViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.RedirectToActionResult>(result);
            var redirectToActionResult = result as Microsoft.AspNetCore.Mvc.RedirectToActionResult;
            Assert.AreEqual("Home", redirectToActionResult.ControllerName);
            Assert.AreEqual(nameof(HomeController.Index), redirectToActionResult.ActionName);
        }

        private ApplicationUser AddExistingUser(string emailAddress, string password, string userName)
        {
            var user = new ApplicationUser()
            {
                Email = emailAddress,
                NormalizedEmail = _normalizer.Normalize(emailAddress),
                UserName = userName,
                NormalizedUserName = _normalizer.Normalize(userName)
            };
            user.PasswordHash = _pwHasher.HashPassword(user, password);
            _userStore.CreateAsync(user).GetAwaiter().GetResult();
            return user;
        }

    }
}