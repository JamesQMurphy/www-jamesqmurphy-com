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

            var user = AddExistingUser(_serviceProvider, email, password, username, true);
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
        public void TestLogin_Fail_WrongEmail()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";

            var user = AddExistingUser(_serviceProvider, email, password, username, true);
            var loginViewModel = new LoginViewModel()
            {
                Email = email + "x",
                Password = password,
                RememberMe = false
            };

            var result = _controller.Login(loginViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
        }

        [Test]
        public void TestLogin_Fail_WrongPassword()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";

            var user = AddExistingUser(_serviceProvider, email, password, username, true);
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

        [Test]
        public void TestLogin_Fail_EmailNotVerified()
        {
            var email = "test@test";
            var username = "user1";
            var password = "abc123";

            var user = AddExistingUser(_serviceProvider, email, password, username, false);
            var loginViewModel = new LoginViewModel()
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            var result = _controller.Login(loginViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            Assert.AreEqual(1, _controller.ModelState.ErrorCount);
        }

        [Test]
        public void TestRegister_Success()
        {
            var email = "test@test";
            var username = "userNew";
            var password = "Abcabc~123";

            var registerViewModel = new RegisterViewModel()
            {
                Email = email,
                Password = password,
                ConfirmPassword = password,
                UserName = username
            };

            var result = _controller.Register(registerViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            var viewResult = result as Microsoft.AspNetCore.Mvc.ViewResult;
            Assert.AreEqual("RegisterConfirmation", viewResult.ViewName);
            Assert.AreEqual(0, _controller.ModelState.ErrorCount);

            // Assert that user was created
            var userStorage = (InMemoryApplicationUserStorage)_serviceProvider.GetService<IApplicationUserStorage>();
            var normalizedUserName = _serviceProvider.GetService<ILookupNormalizer>().Normalize(username);
            var user = userStorage.FindByUserNameAsync(normalizedUserName).GetAwaiter().GetResult();
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.UserName);
            Assert.AreEqual(email, user.Email);
            Assert.IsFalse(user.EmailConfirmed);

            // Assert that verification email was sent
            Assert.AreEqual(1, _emailGenerator.Emails.Count);
            Assert.AreEqual(EmailType.EmailVerification, _emailGenerator.Emails[0].emailType);
        }

        [Test]
        public void TestRegister_EmailPresentButUnverified()
        {
            var email = "test@test";
            var username = "userNew";
            var password = "Abcabc~123";
            AddExistingUser(_serviceProvider, email, password, username, false);

            var passwordReplace = "Defdef~456";
            var registerViewModel = new RegisterViewModel()
            {
                Email = email,
                Password = passwordReplace,
                ConfirmPassword = passwordReplace,
                UserName = username
            };

            var result = _controller.Register(registerViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            var viewResult = result as Microsoft.AspNetCore.Mvc.ViewResult;
            Assert.AreEqual("RegisterConfirmation", viewResult.ViewName);
            Assert.AreEqual(0, _controller.ModelState.ErrorCount);

            // Assert that password was updated
            var userStorage = (InMemoryApplicationUserStorage)_serviceProvider.GetService<IApplicationUserStorage>();
            var normalizedUserName = _serviceProvider.GetService<ILookupNormalizer>().Normalize(username);
            var user = userStorage.FindByUserNameAsync(normalizedUserName).GetAwaiter().GetResult();
            Assert.IsNotNull(user);
            var signinManager = _serviceProvider.GetService<ApplicationSignInManager<ApplicationUser>>();
            var pwVerificationResult = signinManager.UserManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, passwordReplace);
            Assert.AreEqual(PasswordVerificationResult.Success, pwVerificationResult);

            // Assert that verification email was sent
            Assert.AreEqual(1, _emailGenerator.Emails.Count);
            Assert.AreEqual(EmailType.EmailVerification, _emailGenerator.Emails[0].emailType);
        }

        [Test]
        public void TestRegister_EmailPresentButVerified()
        {
            var email = "test@test";
            var username = "userNew";
            var password = "Abcabc~123";
            AddExistingUser(_serviceProvider, email, password, username, true);

            var passwordReplace = "Defdef~456";
            var registerViewModel = new RegisterViewModel()
            {
                Email = email,
                Password = passwordReplace,
                ConfirmPassword = passwordReplace,
                UserName = username
            };

            var result = _controller.Register(registerViewModel).GetAwaiter().GetResult();

            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            var viewResult = result as Microsoft.AspNetCore.Mvc.ViewResult;
            Assert.AreEqual("RegisterConfirmation", viewResult.ViewName);
            Assert.AreEqual(0, _controller.ModelState.ErrorCount);

            // Assert that password was NOT updated
            var userStorage = (InMemoryApplicationUserStorage)_serviceProvider.GetService<IApplicationUserStorage>();
            var normalizedUserName = _serviceProvider.GetService<ILookupNormalizer>().Normalize(username);
            var user = userStorage.FindByUserNameAsync(normalizedUserName).GetAwaiter().GetResult();
            Assert.IsNotNull(user);
            var signinManager = _serviceProvider.GetService<ApplicationSignInManager<ApplicationUser>>();
            var pwVerificationResult = signinManager.UserManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            Assert.AreEqual(PasswordVerificationResult.Success, pwVerificationResult);

            // Assert that "Email already registered" message was sent
            Assert.AreEqual(1, _emailGenerator.Emails.Count);
            Assert.AreEqual(EmailType.EmailAlreadyRegistered, _emailGenerator.Emails[0].emailType);
        }
        private static ApplicationUser AddExistingUser(IServiceProvider serviceProvider, string emailAddress, string password, string userName, bool emailConfirmed)
        {
            var normalizer = serviceProvider.GetService<ILookupNormalizer>();
            var user = new ApplicationUser()
            {
                Email = emailAddress,
                NormalizedEmail = normalizer.Normalize(emailAddress),
                UserName = userName,
                NormalizedUserName = normalizer.Normalize(userName),
                EmailConfirmed = emailConfirmed
            };
            user.PasswordHash = serviceProvider.GetService<IPasswordHasher<ApplicationUser>>().HashPassword(user, password);
            var storage = serviceProvider.GetService<IApplicationUserStorage>();
            storage.CreateAsync(user).GetAwaiter().GetResult();
            return user;
        }

    }
}