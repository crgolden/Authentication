namespace Authentication.Api.Tests.IdentityResources.Edit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Pages.IdentityResources.Edit;
    using Xunit;

    public class ClaimsFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(ClaimsFacts).FullName;
        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public ClaimsFacts()
        {
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
            var identityResource = new IdentityResource
            {
                Id = Random.Next(),
                UserClaims = new List<IdentityClaim>
                {
                    new IdentityClaim(),
                    new IdentityClaim(),
                    new IdentityClaim()
                }
            };
            ClaimTypesModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(identityResource);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new ClaimTypesModel(context);
                get = await model.OnGetAsync(identityResource.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.IdentityResource);
            Assert.Equal(identityResource.Id, model.IdentityResource.Id);
            var claims = Assert.IsAssignableFrom<IEnumerable<IdentityClaim>>(model.ClaimTypes);
            Assert.Equal(identityResource.UserClaims.Count, claims.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new ClaimTypesModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.IdentityResource);
            Assert.Null(model.ClaimTypes);
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
            ClaimTypesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new ClaimTypesModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.IdentityResource);
            Assert.Null(model.ClaimTypes);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string claim1OriginalType = "Original Type";
            const string claim1EditedType = "Edited Type";
            const string newClaimType = "New Type";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            ClaimTypesModel claims;
            IActionResult post;
            var claim1 = new IdentityClaim
            {
                Id = Random.Next(),
                Type = claim1OriginalType
            };
            var claim2 = new IdentityClaim {Id = Random.Next()};
            var identityResourceId = Random.Next();
            var identityResource = new IdentityResource
            {
                Id = identityResourceId,
                UserClaims = new List<IdentityClaim>
                {
                    claim1,
                    claim2
                }
            };
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(identityResource);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                claims = new ClaimTypesModel(context)
                {
                    IdentityResource = new IdentityResource
                    {
                        Id = identityResourceId,
                        UserClaims = new List<IdentityClaim>
                        {
                            new IdentityClaim
                            {
                                Id = claim1.Id,
                                Type = claim1EditedType
                            },
                            new IdentityClaim {Type = newClaimType}
                        }
                    }
                };
                post = await claims.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                identityResource = await context.IdentityResources
                    .Include(x => x.UserClaims)
                    .SingleOrDefaultAsync(x => x.Id.Equals(identityResourceId))
                    .ConfigureAwait(false);
                claim1 = identityResource.UserClaims.SingleOrDefault(x => x.Id.Equals(claim1.Id));
                claim2 = identityResource.UserClaims.SingleOrDefault(x => x.Id.Equals(claim2.Id));
                var newClaim = identityResource.UserClaims.SingleOrDefault(x => x.Type.Equals(newClaimType));

                Assert.NotNull(claim1);
                Assert.Equal(claim1EditedType, claim1.Type);
                Assert.Null(claim2);
                Assert.NotNull(newClaim);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/ClaimTypes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(claims.IdentityResource.Id, value);
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
            ClaimTypesModel claims;
            IActionResult post;
            var identityResourceId = Random.Next();
            var identityResource = new IdentityResource
            {
                Id = identityResourceId,
                UserClaims = new List<IdentityClaim>
                {
                    new IdentityClaim {Id = Random.Next()}
                }
            };
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(identityResource);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                claims = new ClaimTypesModel(context)
                {
                    IdentityResource = new IdentityResource {Id = identityResourceId}
                };
                post = await claims.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                identityResource = await context.IdentityResources
                    .Include(x => x.UserClaims)
                    .SingleOrDefaultAsync(x => x.Id.Equals(identityResourceId))
                    .ConfigureAwait(false);

                Assert.Empty(identityResource.UserClaims);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/ClaimTypes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(claims.IdentityResource.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var claims = new ClaimTypesModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = 0}
            };

            // Act
            var post = await claims.OnPostAsync().ConfigureAwait(false);

            // Assert
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            IActionResult post;
            var identityResource = new IdentityResource {Id = Random.Next()};
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(identityResource);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                var claims = new ClaimTypesModel(context)
                {
                    IdentityResource = new IdentityResource {Id = Random.Next()}
                };
                post = await claims.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
