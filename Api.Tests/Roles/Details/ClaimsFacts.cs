namespace Authentication.Api.Tests.Roles.Details
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using Pages.Roles.Details;
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
            var role = new Role
            {
                Id = Guid.NewGuid()
            };
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
            var role = new Role
            {
                Id = Guid.NewGuid()
            };
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
            var role = new Role
            {
                Id = Guid.NewGuid()
            };
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
    }
}
