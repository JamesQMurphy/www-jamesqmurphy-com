using JamesQMurphy.Auth;
using NUnit.Framework;

namespace JamesQMurphy.Web.UnitTests
{
    public class ApplicationPasswordValidatorTests
    {
        private ApplicationPasswordValidator<ApplicationUser> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new ApplicationPasswordValidator<ApplicationUser>();
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
                _validator.ValidateAsync(null, null, password).GetAwaiter().GetResult().Succeeded
            );
        }
    }
}
