namespace Authentication.Api.Tests.Clients.Edit
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
    using Pages.Clients.Edit;
    using Xunit;

    public class IdPRestrictionsFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(IdPRestrictionsFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public IdPRestrictionsFacts()
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
            var client = new Client
            {
                Id = Random.Next(),
                IdentityProviderRestrictions = new List<ClientIdPRestriction>
                {
                    new ClientIdPRestriction(),
                    new ClientIdPRestriction(),
                    new ClientIdPRestriction()
                }
            };
            IdPRestrictionsModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new IdPRestrictionsModel(context);
                get = await model.OnGetAsync(client.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Client);
            Assert.Equal(client.Id, model.Client.Id);
            var idPRestrictions = Assert.IsAssignableFrom<IEnumerable<ClientIdPRestriction>>(model.IdPRestrictions);
            Assert.Equal(client.IdentityProviderRestrictions.Count, idPRestrictions.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new IdPRestrictionsModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.IdPRestrictions);
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
            IdPRestrictionsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new IdPRestrictionsModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.IdPRestrictions);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string idPRestriction1OriginalProvider = "Original Provider";
            const string idPRestriction1EditedProvider = "Edited Provider";
            const string newIdPRestrictionProvider = "New Provider";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            IdPRestrictionsModel idPRestrictions;
            IActionResult post;
            var idPRestriction1 = new ClientIdPRestriction
            {
                Id = Random.Next(),
                Provider = idPRestriction1OriginalProvider
            };
            var idPRestriction2 = new ClientIdPRestriction {Id = Random.Next()};
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                IdentityProviderRestrictions = new List<ClientIdPRestriction>
                {
                    idPRestriction1,
                    idPRestriction2
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
                idPRestrictions = new IdPRestrictionsModel(context)
                {
                    Client = new Client
                    {
                        Id = clientId,
                        IdentityProviderRestrictions = new List<ClientIdPRestriction>
                        {
                            new ClientIdPRestriction
                            {
                                Id = idPRestriction1.Id,
                                Provider = idPRestriction1EditedProvider
                            },
                            new ClientIdPRestriction {Provider = newIdPRestrictionProvider}
                        }
                    }
                };
                post = await idPRestrictions.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.IdentityProviderRestrictions)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);
                idPRestriction1 = client.IdentityProviderRestrictions.SingleOrDefault(x => x.Id.Equals(idPRestriction1.Id));
                idPRestriction2 = client.IdentityProviderRestrictions.SingleOrDefault(x => x.Id.Equals(idPRestriction2.Id));
                var newIdPRestriction = client.IdentityProviderRestrictions.SingleOrDefault(x => x.Provider.Equals(newIdPRestrictionProvider));

                Assert.NotNull(idPRestriction1);
                Assert.Equal(idPRestriction1EditedProvider, idPRestriction1.Provider);
                Assert.Null(idPRestriction2);
                Assert.NotNull(newIdPRestriction);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/IdPRestrictions", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(idPRestrictions.Client.Id, value);
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
            IdPRestrictionsModel idPRestrictions;
            IActionResult post;
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                IdentityProviderRestrictions = new List<ClientIdPRestriction>
                {
                    new ClientIdPRestriction {Id = Random.Next()}
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
                idPRestrictions = new IdPRestrictionsModel(context)
                {
                    Client = new Client {Id = clientId}
                };
                post = await idPRestrictions.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.IdentityProviderRestrictions)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);

                Assert.Empty(client.IdentityProviderRestrictions);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/IdPRestrictions", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(idPRestrictions.Client.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var idPRestrictions = new IdPRestrictionsModel(context.Object)
            {
                Client = new Client {Id = 0}
            };

            // Act
            var post = await idPRestrictions.OnPostAsync().ConfigureAwait(false);

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
            var client = new Client {Id = Random.Next()};
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                var idPRestrictions = new IdPRestrictionsModel(context)
                {
                    Client = new Client {Id = Random.Next()}
                };
                post = await idPRestrictions.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
