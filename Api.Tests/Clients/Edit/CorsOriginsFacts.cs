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

    public class CorsOriginsFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(CorsOriginsFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public CorsOriginsFacts()
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
                AllowedCorsOrigins = new List<ClientCorsOrigin>
                {
                    new ClientCorsOrigin(),
                    new ClientCorsOrigin(),
                    new ClientCorsOrigin()
                }
            };
            CorsOriginsModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new CorsOriginsModel(context);
                get = await model.OnGetAsync(client.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Client);
            Assert.Equal(client.Id, model.Client.Id);
            var corsOrigins = Assert.IsAssignableFrom<IEnumerable<ClientCorsOrigin>>(model.CorsOrigins);
            Assert.Equal(client.AllowedCorsOrigins.Count, corsOrigins.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new CorsOriginsModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.CorsOrigins);
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
            CorsOriginsModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new CorsOriginsModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.CorsOrigins);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string corsOrigin1OriginalOrigin = "Original Origin";
            const string corsOrigin1EditedOrigin = "Edited Origin";
            const string newCorsOriginOrigin = "New Origin";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            CorsOriginsModel corsOrigins;
            IActionResult post;
            var corsOrigin1 = new ClientCorsOrigin
            {
                Id = Random.Next(),
                Origin = corsOrigin1OriginalOrigin
            };
            var corsOrigin2 = new ClientCorsOrigin {Id = Random.Next()};
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                AllowedCorsOrigins = new List<ClientCorsOrigin>
                {
                    corsOrigin1,
                    corsOrigin2
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
                corsOrigins = new CorsOriginsModel(context)
                {
                    Client = new Client
                    {
                        Id = clientId,
                        AllowedCorsOrigins = new List<ClientCorsOrigin>
                        {
                            new ClientCorsOrigin
                            {
                                Id = corsOrigin1.Id,
                                Origin = corsOrigin1EditedOrigin
                            },
                            new ClientCorsOrigin {Origin = newCorsOriginOrigin}
                        }
                    }
                };
                post = await corsOrigins.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.AllowedCorsOrigins)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);
                corsOrigin1 = client.AllowedCorsOrigins.SingleOrDefault(x => x.Id.Equals(corsOrigin1.Id));
                corsOrigin2 = client.AllowedCorsOrigins.SingleOrDefault(x => x.Id.Equals(corsOrigin2.Id));
                var newCorsOrigin = client.AllowedCorsOrigins.SingleOrDefault(x => x.Origin.Equals(newCorsOriginOrigin));

                Assert.NotNull(corsOrigin1);
                Assert.Equal(corsOrigin1EditedOrigin, corsOrigin1.Origin);
                Assert.Null(corsOrigin2);
                Assert.NotNull(newCorsOrigin);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/CorsOrigins", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(corsOrigins.Client.Id, value);
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
            CorsOriginsModel corsOrigins;
            IActionResult post;
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                AllowedCorsOrigins = new List<ClientCorsOrigin>
                {
                    new ClientCorsOrigin {Id = Random.Next()}
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
                corsOrigins = new CorsOriginsModel(context)
                {
                    Client = new Client {Id = client.Id}
                };
                post = await corsOrigins.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.AllowedCorsOrigins)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);

                Assert.Empty(client.AllowedCorsOrigins);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/CorsOrigins", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(corsOrigins.Client.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var corsOrigins = new CorsOriginsModel(context.Object)
            {
                Client = new Client {Id = 0}
            };

            // Act
            var post = await corsOrigins.OnPostAsync().ConfigureAwait(false);

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
                var corsOrigins = new CorsOriginsModel(context)
                {
                    Client = new Client {Id = Random.Next()}
                };
                post = await corsOrigins.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
