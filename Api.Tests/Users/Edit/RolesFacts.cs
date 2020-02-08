namespace Authentication.Api.Tests.Users.Edit
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
    using Pages.Users.Edit;
    using Xunit;

    public class RolesFacts
    {
        private static string DatabaseNamePrefix => typeof(RolesFacts).FullName;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public RolesFacts()
        {
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
            _roleManager = new Mock<RoleManager<Role>>(
                Mock.Of<IRoleStore<Role>>(),
                new List<IRoleValidator<Role>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<ILogger<RoleManager<Role>>>());
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
            var user = new User();
            user.UserRoles.Add(new UserRole { Role = new Role() });
            user.UserRoles.Add(new UserRole { Role = new Role() });
            user.UserRoles.Add(new UserRole { Role = new Role() });
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var rolesList = await context.Roles.ToListAsync().ConfigureAwait(false);
                _roleManager.Setup(x => x.Roles).Returns(rolesList.AsQueryable());
            }

            RolesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new RolesModel(_userManager.Object, _roleManager.Object);
                get = await model.OnGetAsync(user.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.UserModel);
            Assert.Equal(user.Id, model.UserModel.Id);
            var roles = Assert.IsAssignableFrom<IEnumerable<Role>>(model.Roles);
            Assert.Equal(user.UserRoles.Count, roles.Count());
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
            var user = new User();
            var id = Guid.Empty;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            RolesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new RolesModel(_userManager.Object, _roleManager.Object);
                get = await model.OnGetAsync(id).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.UserModel);
            Assert.Null(model.Roles);
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
            var user = new User();
            var id = Guid.NewGuid();
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            RolesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new RolesModel(_userManager.Object, _roleManager.Object);
                get = await model.OnGetAsync(id).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.UserModel);
            Assert.Null(model.Roles);
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
            var role1 = new Role {Name = "Role 1 Name"};
            var role2 = new Role {Name = "Role 2 Name"};
            var userRole = new UserRole {Role = role1};
            var user = new User
            {
                Email = "Email",
            };
            user.UserRoles.Add(userRole);
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                context.Add(role2);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var rolesList = await context.Roles.ToListAsync().ConfigureAwait(false);
                _roleManager.Setup(x => x.Roles).Returns(rolesList.AsQueryable());
                foreach (var role in context.Roles)
                {
                    _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
                }
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                var model = new RolesModel(_userManager.Object, _roleManager.Object) {UserModel = user};

                model.UserModel.UserRoles.Remove(userRole);
                model.UserModel.UserRoles.Add(new UserRole {RoleId = role2.Id});
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(
                x => x.AddClaimAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role2.Name))),
                Times.Once);
            _userManager.Verify(
                x => x.RemoveClaimAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role1.Name))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Roles", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(User.Id), key);
                Assert.Equal(user.Id, value);
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
            var role = new Role {Name = "Role Name"};
            var user = new User
            {
                Email = "Email"
            };
            user.Claims.Add(new UserClaim
            {
                ClaimType = ClaimTypes.Role,
                ClaimValue = role.Name
            });
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
                var rolesList = await context.Roles.ToListAsync().ConfigureAwait(false);
                _roleManager.Setup(x => x.Roles).Returns(rolesList.AsQueryable());
            }

            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                var model = new RolesModel(_userManager.Object, _roleManager.Object)
                {
                    UserModel = new User {Id = user.Id}
                };
                get = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(
                x => x.RemoveClaimsAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<IEnumerable<Claim>>(y => y.SingleOrDefault(z => z.Type.Equals(ClaimTypes.Role) && z.Value.Equals(role.Name)) != null)),
                Times.Once);

            _userManager.Verify(
                x => x.UpdateAsync(It.Is<User>(y => y.Id.Equals(user.Id) && y.UserRoles.Count == 0)),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(get);
            Assert.Equal("../Details/Roles", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(User.Id), key);
                Assert.Equal(user.Id, value);
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
            var user = new User {Id = Guid.NewGuid()};
            var id = Guid.Empty;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                var model = new RolesModel(_userManager.Object, _roleManager.Object)
                {
                    UserModel = new User {Id = id}
                };
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
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
            var user = new User {Id = Guid.NewGuid()};
            var id = Guid.NewGuid();
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult post;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                var model = new RolesModel(_userManager.Object, _roleManager.Object)
                {
                    UserModel = new User {Id = id}
                };
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
