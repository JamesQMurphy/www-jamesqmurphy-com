using System;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.ProfileViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace JamesQMurphy.Web.UnitTests
{
    public class ProfileControllerTests
    {
        private ServiceProvider _serviceProvider = ConfigurationHelper.CreateServiceProvider();
        private MockEmailGenerator _emailGenerator = new MockEmailGenerator();
        private ProfileController _controller;

        [SetUp]
        public void Setup()
        {
            _emailGenerator.Emails.Clear();

            _controller = new ProfileController(
                _serviceProvider.GetService<ApplicationSignInManager<ApplicationUser>>(),
                _serviceProvider.GetService<ILogger<ProfileController>>(),
                _emailGenerator);
        }

        [Test]
        public void TestLogin_Success()
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

        [Test]
        public void TestLogin_Fail_WrongPassword()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";

            var user = AddExistingUser(_serviceProvider, email, password, username);
            var loginViewModel = new LoginViewModel()
            {
                Email = email,
                Password = password + "x",
                RememberMe = false
            };

            var result = _controller.Login(loginViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            var viewResult = result as Microsoft.AspNetCore.Mvc.ViewResult;
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