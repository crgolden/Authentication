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

    public class ClaimsFacts
    {
        private static string DatabaseNamePrefix => typeof(ClaimsFacts).FullName;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public ClaimsFacts()
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
            user.Claims.Add(new UserClaim { ClaimType = "Claim 1 Type" });
            user.Claims.Add(new UserClaim { ClaimType = "Claim 2 Type" });
            user.Claims.Add(new UserClaim { ClaimType = "Claim 3 Type" });
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new ClaimsModel(_userManager.Object);
                get = await model.OnGetAsync(user.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.UserModel);
            Assert.Equal(user.Id, model.UserModel.Id);
            var claims = Assert.IsAssignableFrom<IEnumerable<UserClaim>>(model.Claims);
            Assert.Equal(user.Claims.Count, claims.Count());
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
            user.Claims.Add(new UserClaim());
            user.Claims.Add(new UserClaim());
            user.Claims.Add(new UserClaim());
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new ClaimsModel(_userManager.Object);
                get = await model.OnGetAsync(Guid.Empty).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.UserModel);
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
            var user = new User();
            user.Claims.Add(new UserClaim());
            user.Claims.Add(new UserClaim());
            user.Claims.Add(new UserClaim());
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            ClaimsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                model = new ClaimsModel(_userManager.Object);
                get = await model.OnGetAsync(Guid.NewGuid()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.UserModel);
            Assert.Null(model.Claims);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string originalUserClaim2Type = "Claim 2 Type";
            const string newUserClaim2Type = "New Claim Type";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var userClaim1 = new UserClaim
            {
                ClaimType = "Claim 1 Type",
                ClaimValue = "Claim 1 Value"
            };
            var userClaim2 = new UserClaim
            {
                ClaimType = originalUserClaim2Type,
                ClaimValue = "Claim 2 Value"
            };
            var userClaim3 = new UserClaim
            {
                ClaimType = "Claim 3 Type",
                ClaimValue = "Claim 3 Value"
            };
            var userClaim4 = new UserClaim
            {
                ClaimType = "Claim 4 Type",
                ClaimValue = "Claim 4 Value"
            };
            var user = new User
            {
                Email = "Email"
            };
            user.Claims.Add(userClaim1);
            user.Claims.Add(userClaim2);
            user.Claims.Add(userClaim3);
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
                var model = new ClaimsModel(_userManager.Object) {UserModel = user};
                model.UserModel.Claims.Single(x => x.ClaimType.Equals(userClaim2.ClaimType)).ClaimType = newUserClaim2Type;
                model.UserModel.Claims.Remove(userClaim3);
                model.UserModel.Claims.Add(userClaim4);
                post = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(x => x.ReplaceClaimAsync(
                It.Is<User>(y => y.Id.Equals(user.Id)),
                It.Is<Claim>(y => y.Type.Equals(userClaim1.ClaimType) && y.Value.Equals(userClaim1.ClaimValue)),
                It.IsAny<Claim>()),
                Times.Never);
            _userManager.Verify(x => x.ReplaceClaimAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<Claim>(y => y.Type.Equals(originalUserClaim2Type) && y.Value.Equals(userClaim2.ClaimValue)),
                    It.Is<Claim>(y => y.Type.Equals(newUserClaim2Type) && y.Value.Equals(userClaim2.ClaimValue))),
                Times.Once);
            _userManager.Verify(
                x => x.AddClaimsAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<IEnumerable<Claim>>(y => y.SingleOrDefault(z => z.Type.Equals(userClaim4.ClaimType) && z.Value.Equals(userClaim4.ClaimValue)) != null)),
                Times.Once);
            _userManager.Verify(
                x => x.RemoveClaimsAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<IEnumerable<Claim>>(y => y.SingleOrDefault(z => z.Type.Equals(userClaim3.ClaimType) && z.Value.Equals(userClaim3.ClaimValue)) != null)),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Claims", result.PageName);
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
            var userClaim = new UserClaim
            {
                ClaimType = "Claim Type",
                ClaimValue = "Claim Value"
            };
            var user = new User
            {
                Email = "Email"
            };
            user.Claims.Add(userClaim);
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                _userManager.Setup(x => x.Users).Returns(context.Users);
                var model = new ClaimsModel(_userManager.Object)
                {
                    UserModel = new User {Id = user.Id}
                };
                get = await model.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            _userManager.Verify(
                x => x.RemoveClaimsAsync(
                    It.Is<User>(y => y.Id.Equals(user.Id)),
                    It.Is<IEnumerable<Claim>>(y => y.SingleOrDefault(z => z.Type.Equals(userClaim.ClaimType) && z.Value.Equals(userClaim.ClaimValue)) != null)),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(get);
            Assert.Equal("../Details/Claims", result.PageName);
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
                var model = new ClaimsModel(_userManager.Object)
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
                var model = new ClaimsModel(_userManager.Object)
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
