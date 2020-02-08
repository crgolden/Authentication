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

    public class ClaimsFacts
    {
        private static string DatabaseNamePrefix => typeof(ClaimsFacts).FullName;
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public ClaimsFacts()
        {
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
            var role = new Role();
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new ClaimsModel(_roleManager.Object);
                get = await model.OnGetAsync(role.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Role);
            Assert.Equal(role.Id, model.Role.Id);
            var claims = Assert.IsAssignableFrom<IEnumerable<RoleClaim>>(model.Claims);
            Assert.Equal(role.RoleClaims.Count, claims.Count());
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
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new ClaimsModel(_roleManager.Object);
                get = await model.OnGetAsync(Guid.Empty).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Role);
            Assert.Null(model.Claims);
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
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            role.RoleClaims.Add(new RoleClaim());
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                model = new ClaimsModel(_roleManager.Object);
                get = await model.OnGetAsync(Guid.NewGuid()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Role);
            Assert.Null(model.Claims);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string originalRoleClaim2Type = "Claim 2 Type";
            const string newRoleClaim2Type = "New Claim Type";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var roleClaim1 = new RoleClaim
            {
                ClaimType = "Claim 1 Type",
                ClaimValue = "Claim 1 Value"
            };
            var roleClaim2 = new RoleClaim
            {
                ClaimType = originalRoleClaim2Type,
                ClaimValue = "Claim 2 Value"
            };
            var roleClaim3 = new RoleClaim
            {
                ClaimType = "Claim 3 Type",
                ClaimValue = "Claim 3 Value"
            };
            var roleClaim4 = new RoleClaim
            {
                ClaimType = "Claim 4 Type",
                ClaimValue = "Claim 4 Value"
            };
            var role = new Role
            {
                Name = "Role Name"
            };
            role.RoleClaims.Add(roleClaim1);
            role.RoleClaims.Add(roleClaim2);
            role.RoleClaims.Add(roleClaim3);
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
                var model = new ClaimsModel(_roleManager.Object) {Role = role};
                model.Role.RoleClaims.Single(x => x.ClaimType.Equals(roleClaim2.ClaimType)).ClaimType = newRoleClaim2Type;
                model.Role.RoleClaims.Remove(roleClaim3);
                model.Role.RoleClaims.Add(roleClaim4);
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _roleManager.Verify(x => x.RemoveClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(roleClaim1.ClaimType) && y.Value.Equals(roleClaim1.ClaimValue))),
                Times.Never);
            _roleManager.Verify(x => x.AddClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(roleClaim1.ClaimType) && y.Value.Equals(roleClaim1.ClaimValue))),
                Times.Never);
            _roleManager.Verify(x => x.RemoveClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(originalRoleClaim2Type) && y.Value.Equals(roleClaim2.ClaimValue))),
                Times.Once);
            _roleManager.Verify(x => x.AddClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(newRoleClaim2Type) && y.Value.Equals(roleClaim2.ClaimValue))),
                Times.Once);
            _roleManager.Verify(
                x => x.AddClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(roleClaim4.ClaimType) && y.Value.Equals(roleClaim4.ClaimValue))),
                Times.Once);
            _roleManager.Verify(
                x => x.RemoveClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(roleClaim3.ClaimType) && y.Value.Equals(roleClaim3.ClaimValue))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Claims", result.PageName);
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
            var roleClaim = new RoleClaim
            {
                ClaimType = "Claim Type",
                ClaimValue = "Claim Value"
            };
            var role = new Role
            {
                Name = "Role Name"
            };
            role.RoleClaims.Add(roleClaim);
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(role);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _roleManager.Setup(x => x.Roles).Returns(context.Roles);
                var model = new ClaimsModel(_roleManager.Object)
                {
                    Role = new Role {Id = role.Id}
                };
                get = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _roleManager.Verify(
                x => x.RemoveClaimAsync(
                    It.Is<Role>(y => y.Id.Equals(role.Id)),
                    It.Is<Claim>(y => y.Type.Equals(roleClaim.ClaimType) && y.Value.Equals(roleClaim.ClaimValue))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(get);
            Assert.Equal("../Details/Claims", result.PageName);
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
                var model = new ClaimsModel(_roleManager.Object)
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
                var model = new ClaimsModel(_roleManager.Object)
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
