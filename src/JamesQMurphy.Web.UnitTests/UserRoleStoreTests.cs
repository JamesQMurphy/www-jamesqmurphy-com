using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Services;
using NUnit.Framework;
using System.Linq;
using System.Threading;


namespace JamesQMurphy.Web.UnitTests
{
    public class UserRoleStoreTests
    {
        private ApplicationUserStore _applicationUserStore;
        private InMemoryApplicationUserStorage _inMemoryApplicationUserStorage;

        [SetUp]
        public void Setup()
        {
            _inMemoryApplicationUserStorage = new InMemoryApplicationUserStorage();
            var users = _inMemoryApplicationUserStorage.GetAllUsersAsync().GetAwaiter().GetResult().ToList();
            foreach (var user in users)
            {
                _inMemoryApplicationUserStorage.DeleteAsync(user);
            }

            _applicationUserStore = new ApplicationUserStore(_inMemoryApplicationUserStorage);
        }

        [Test]
        public void ConfirmedUsersAreRegisteredUsersButNotAdmins()
        {
            var userConfirmed1 = new ApplicationUser()
            {
                Email = "confirmed1@b",
                NormalizedEmail = "CONFIRMED1@B",
                UserName = "userConfirmed1",
                NormalizedUserName = "USERCONFIRMED1",
                EmailConfirmed = true,
                IsAdministrator = false
            };

            Assert.IsTrue(_applicationUserStore.IsInRoleAsync(userConfirmed1, ApplicationRole.RegisteredUser.Name, CancellationToken.None).GetAwaiter().GetResult());
            Assert.IsFalse(_applicationUserStore.IsInRoleAsync(userConfirmed1, ApplicationRole.Administrator.Name, CancellationToken.None).GetAwaiter().GetResult());

            var listRoles = _applicationUserStore.GetRolesAsync(userConfirmed1, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(listRoles.Contains(ApplicationRole.RegisteredUser.Name));
            Assert.IsFalse(listRoles.Contains(ApplicationRole.Administrator.Name));

        }

        [Test]
        public void UnconfirmedUsersAreNotRegisteredUsersNorAdmins()
        {
            var userUnconfirmed = new ApplicationUser()
            {
                Email = "unconfirmed@b",
                NormalizedEmail = "UNCONFIRMED@B",
                UserName = "Unconfirmed",
                NormalizedUserName = "UNCONFIRMED",
                EmailConfirmed = false,
                IsAdministrator = false
            };

            Assert.IsFalse(_applicationUserStore.IsInRoleAsync(userUnconfirmed, ApplicationRole.RegisteredUser.Name, CancellationToken.None).GetAwaiter().GetResult());
            Assert.IsFalse(_applicationUserStore.IsInRoleAsync(userUnconfirmed, ApplicationRole.Administrator.Name, CancellationToken.None).GetAwaiter().GetResult());

            var listRoles = _applicationUserStore.GetRolesAsync(userUnconfirmed, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsFalse(listRoles.Contains(ApplicationRole.RegisteredUser.Name));
            Assert.IsFalse(listRoles.Contains(ApplicationRole.Administrator.Name));
        }


        [Test]
        public void AdminsAreRegisteredUsersAndAdmins()
        {
            var userAdministrator = new ApplicationUser()
            {
                Email = "admin@b",
                NormalizedEmail = "ADMIN@B",
                UserName = "Administrator",
                NormalizedUserName = "ADMINISTRATOR",
                EmailConfirmed = true,
                IsAdministrator = true
            };

            Assert.IsTrue(_applicationUserStore.IsInRoleAsync(userAdministrator, ApplicationRole.RegisteredUser.Name, CancellationToken.None).GetAwaiter().GetResult());
            Assert.IsTrue(_applicationUserStore.IsInRoleAsync(userAdministrator, ApplicationRole.Administrator.Name, CancellationToken.None).GetAwaiter().GetResult());

            var listRoles = _applicationUserStore.GetRolesAsync(userAdministrator, CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(listRoles.Contains(ApplicationRole.RegisteredUser.Name));
            Assert.IsTrue(listRoles.Contains(ApplicationRole.Administrator.Name));
        }
    }
}