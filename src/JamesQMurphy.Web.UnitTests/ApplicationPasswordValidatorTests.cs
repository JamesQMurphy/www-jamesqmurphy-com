using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace JamesQMurphy.Web.UnitTests
{
    public class ApplicationPasswordValidatorTests
    {
        private ApplicationPasswordValidator<ApplicationUser> _validator;
        private UserManager<ApplicationUser> _userManager;

        [SetUp]
        public void Setup()
        {
            _validator = new ApplicationPasswordValidator<ApplicationUser>();
            var serviceProvider = ConfigurationHelper.CreateServiceProvider();
            _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        }

        [Test]
        public void GoodPasswords()
        {
            TestPassword("Abcdef123~", true);

            TestPassword("Abcde1", true);
            TestPassword("abcD3f", true);
            TestPassword("abcD-f", true);
            TestPassword("----bC", true);
        }

        [Test]
        public void BadPasswords()
        {
            TestPassword("x", false);
            TestPassword("Sh0rt", false);
            TestPassword("NO_L0WERCASE", false);
            TestPassword("n0-uppercase", false);
            TestPassword("NoNumbersOrSymbols", false);
            TestPassword("01294582034589", false);
        }

        private void TestPassword(string password, bool expected)
        {
            Assert.AreEqual(
                expected,
                _validator.ValidateAsync(_userManager, null, password).GetAwaiter().GetResult().Succeeded
            );
        }
    }
}
