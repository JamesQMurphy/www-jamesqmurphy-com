using System;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Controllers;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AccountViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace JamesQMurphy.Web.UnitTests
{
    public class AccountControllerTests
    {
        private ServiceProvider _serviceProvider;
        private MockEmailGenerator _emailGenerator;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _emailGenerator = new MockEmailGenerator();
            _serviceProvider = ConfigurationHelper.CreateServiceProvider();

            _controller = new AccountController(
                _serviceProvider.GetService<ApplicationSignInManager<ApplicationUser>>(),
                _serviceProvider.GetService<ILogger<AccountController>>(),
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
            Assert.AreEqual(0, _controller.ModelState.ErrorCount);
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
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
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