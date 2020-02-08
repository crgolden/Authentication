namespace Authentication.Api.Tests.ApiResources.Edit
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
    using Pages.ApiResources.Edit;
    using Xunit;

    public class ScopesFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(ScopesFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public ScopesFacts()
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
            var client = new ApiResource
            {
                Id = Random.Next(),
                Scopes = new List<ApiScope>
                {
                    new ApiScope(),
                    new ApiScope(),
                    new ApiScope()
                }
            };
            ScopesModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new ScopesModel(context);
                get = await model.OnGetAsync(client.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.ApiResource);
            Assert.Equal(client.Id, model.ApiResource.Id);
            var scopes = Assert.IsAssignableFrom<IEnumerable<ApiScope>>(model.Scopes);
            Assert.Equal(client.Scopes.Count, scopes.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new ScopesModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.ApiResource);
            Assert.Null(model.Scopes);
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
            ScopesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new ScopesModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.ApiResource);
            Assert.Null(model.Scopes);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string scope1OriginalName = "Original Name";
            const string scope1EditedName = "Edited Name";
            const string newScopeName = "New Name";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            ScopesModel scopes;
            IActionResult post;
            var scope1 = new ApiScope
            {
                Id = Random.Next(),
                Name = scope1OriginalName
            };
            var scope2 = new ApiScope {Id = Random.Next()};
            var clientId = Random.Next();
            var client = new ApiResource
            {
                Id = clientId,
                Scopes = new List<ApiScope>
                {
                    scope1,
                    scope2
                }
            };
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                scopes = new ScopesModel(context)
                {
                    ApiResource = new ApiResource
                    {
                        Id = clientId,
                        Scopes = new List<ApiScope>
                        {
                            new ApiScope
                            {
                                Id = scope1.Id,
                                Name = scope1EditedName
                            },
                            new ApiScope {Name = newScopeName}
                        }
                    }
                };
                post = await scopes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.ApiResources
                    .Include(x => x.Scopes)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);
                scope1 = client.Scopes.SingleOrDefault(x => x.Id.Equals(scope1.Id));
                scope2 = client.Scopes.SingleOrDefault(x => x.Id.Equals(scope2.Id));
                var newScope = client.Scopes.SingleOrDefault(x => x.Name.Equals(newScopeName));

                Assert.NotNull(scope1);
                Assert.Equal(scope1EditedName, scope1.Name);
                Assert.Null(scope2);
                Assert.NotNull(newScope);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Scopes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(ApiResource.Id), key);
                Assert.Equal(scopes.ApiResource.Id, value);
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
            ScopesModel scopes;
            IActionResult post;
            var clientId = Random.Next();
            var client = new ApiResource
            {
                Id = clientId,
                Scopes = new List<ApiScope>
                {
                    new ApiScope {Id = Random.Next()}
                }
            };
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                scopes = new ScopesModel(context)
                {
                    ApiResource = new ApiResource {Id = clientId}
                };
                post = await scopes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.ApiResources
                    .Include(x => x.Scopes)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);

                Assert.Empty(client.Scopes);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Scopes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(ApiResource.Id), key);
                Assert.Equal(scopes.ApiResource.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var scopes = new ScopesModel(context.Object)
            {
                ApiResource = new ApiResource {Id = 0}
            };

            // Act
            var post = await scopes.OnPostAsync().ConfigureAwait(false);

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
            var client = new ApiResource {Id = Random.Next()};
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                var scopes = new ScopesModel(context)
                {
                    ApiResource = new ApiResource {Id = Random.Next()}
                };
                post = await scopes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
