namespace Authentication.Api.Tests.Roles.Edit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core;
    using Core.Entities;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Pages.Roles.Edit;
    using Xunit;

    public class UsersFacts
    {
        private static string DatabaseNamePrefix => typeof(UsersFacts).FullName;
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public UsersFacts()
        {
            _roleManager = new Mock<RoleManager<Role>>(
                Mock.Of<IRoleStore<Role>>(),
                new List<IRoleValidator<Role>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<ILogger<RoleManager<Role>>>());
            _userManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>());
            _configurationStoreOptions = new Mock<IOptionsSnapshot<ConfigurationStoreOptions>>();
            _configurationStoreOptions.Setup(x => x.Value).Returns(new ConfigurationStoreOptions());
            _operationalStoreOptions = new Mock<IOptionsSnapshot<OperationalStoreOptions>>();
            _operationalStoreOptions.Setup(x => x.Value).Returns(new OperationalStoreOptions());
        }

        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnGetAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role();
            role.UserRoles.Add(new UserRole { User = new User() });
            role.UserRoles.Add(new UserRole { User = new User() });
            role.UserRoles.Add(new UserRole { User = new User() });
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var usersList = await context.Users.ToListAsync().ConfigureAwait(false);
                _userManager.Setup(x => x.Users).Returns(usersList.AsQueryable());
            }

            UsersModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new UsersModel(_roleManager.Object, _userManager.Object);
                get = await model.OnGetAsync(role.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Role);
            Assert.Equal(role.Id, model.Role.Id);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(model.Users);
            Assert.Equal(role.UserRoles.Count, users.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnGetAsync_InvalidId)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role();
            var id = Guid.Empty;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            UsersModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new UsersModel(_roleManager.Object, _userManager.Object);
                get = await model.OnGetAsync(id).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Role);
            Assert.Null(model.Users);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnGetAsync_InvalidModel)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role();
            var id = Guid.NewGuid();
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            UsersModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new UsersModel(_roleManager.Object, _userManager.Object);
                get = await model.OnGetAsync(id).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Role);
            Assert.Null(model.Users);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var user1 = new User();
            var user2 = new User();
            var userRole = new UserRole {User = user1};
            var role = new Role
            {
                Name = "Role Name"
            };
            role.UserRoles.Add(userRole);
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                context.Add(user2);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var usersList = await context.Users.ToListAsync().ConfigureAwait(false);
                _userManager.Setup(x => x.Users).Returns(usersList.AsQueryable());
                foreach (var user in context.Users)
                {
                    _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
                }
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                var model = new UsersModel(_roleManager.Object, _userManager.Object) {Role = role};

                model.Role.UserRoles.Remove(userRole);
                model.Role.UserRoles.Add(new UserRole {UserId = user2.Id});
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(
                x => x.AddClaimAsync(
                    user2,
                    It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role.Name))),
                Times.Once);
            _userManager.Verify(
                x => x.RemoveClaimAsync(
                    user1,
                    It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role.Name))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Users", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Role.Id), key);
                Assert.Equal(role.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_AllRemoved()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync_AllRemoved)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role
            {
                Name = "Role Name"
            };
            role.UserRoles.Add(new UserRole { User = new User() });
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var usersList = await context.Users.ToListAsync().ConfigureAwait(false);
                _userManager.Setup(x => x.Users).Returns(usersList.AsQueryable());
                _userManager.Setup(x => x.GetUsersInRoleAsync(role.Name)).ReturnsAsync(usersList);
            }

            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                var model = new UsersModel(_roleManager.Object, _userManager.Object)
                {
                    Role = new Role {Id = role.Id}
                };
                get = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            foreach (var user in _userManager.Object.Users)
            {
                _userManager.Verify(
                    x => x.RemoveClaimAsync(
                        user,
                        It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role.Name))),
                    Times.Once);
            }

            _roleManager.Verify(
                x => x.UpdateAsync(It.Is<Role>(y => y.Id.Equals(role.Id) && y.UserRoles.Count == 0)),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(get);
            Assert.Equal("../Details/Users", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Role.Id), key);
                Assert.Equal(role.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync_InvalidId)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role {Id = Guid.NewGuid()};
            var id = Guid.Empty;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                var model = new UsersModel(_roleManager.Object, _userManager.Object)
                {
                    Role = new Role {Id = id}
                };
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _roleManager.Verify(x => x.UpdateAsync(It.IsAny<Role>()), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync_InvalidModel)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var role = new Role {Id = Guid.NewGuid()};
            var id = Guid.NewGuid();
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                var model = new UsersModel(_roleManager.Object, _userManager.Object)
                {
                    Role = new Role {Id = id}
                };
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _roleManager.Verify(x => x.UpdateAsync(It.IsAny<Role>()), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
