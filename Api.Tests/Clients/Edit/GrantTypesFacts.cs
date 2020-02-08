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

    public class GrantTypesFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(GrantTypesFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public GrantTypesFacts()
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
                AllowedGrantTypes = new List<ClientGrantType>
                {
                    new ClientGrantType(),
                    new ClientGrantType(),
                    new ClientGrantType()
                }
            };
            GrantTypesModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new GrantTypesModel(context);
                get = await model.OnGetAsync(client.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Client);
            Assert.Equal(client.Id, model.Client.Id);
            var grantTypes = Assert.IsAssignableFrom<IEnumerable<ClientGrantType>>(model.GrantTypes);
            Assert.Equal(client.AllowedGrantTypes.Count, grantTypes.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new GrantTypesModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.GrantTypes);
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
            GrantTypesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new GrantTypesModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.GrantTypes);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string grantType1OriginalGrantType = "Original GrantType";
            const string grantType1EditedGrantType = "Edited GrantType";
            const string newGrantTypeGrantType = "New GrantType";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            GrantTypesModel grantTypes;
            IActionResult post;
            var grantType1 = new ClientGrantType
            {
                Id = Random.Next(),
                GrantType = grantType1OriginalGrantType
            };
            var grantType2 = new ClientGrantType {Id = Random.Next()};
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                AllowedGrantTypes = new List<ClientGrantType>
                {
                    grantType1,
                    grantType2
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
                grantTypes = new GrantTypesModel(context)
                {
                    Client = new Client
                    {
                        Id = clientId,
                        AllowedGrantTypes = new List<ClientGrantType>
                        {
                            new ClientGrantType
                            {
                                Id = grantType1.Id,
                                GrantType = grantType1EditedGrantType
                            },
                            new ClientGrantType {GrantType = newGrantTypeGrantType}
                        }
                    }
                };
                post = await grantTypes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.AllowedGrantTypes)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);
                grantType1 = client.AllowedGrantTypes.SingleOrDefault(x => x.Id.Equals(grantType1.Id));
                grantType2 = client.AllowedGrantTypes.SingleOrDefault(x => x.Id.Equals(grantType2.Id));
                var newGrantType = client.AllowedGrantTypes.SingleOrDefault(x => x.GrantType.Equals(newGrantTypeGrantType));

                Assert.NotNull(grantType1);
                Assert.Equal(grantType1EditedGrantType, grantType1.GrantType);
                Assert.Null(grantType2);
                Assert.NotNull(newGrantType);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/GrantTypes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(grantTypes.Client.Id, value);
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
            GrantTypesModel grantTypes;
            IActionResult post;
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                AllowedGrantTypes = new List<ClientGrantType>
                {
                    new ClientGrantType {Id = Random.Next()}
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
                grantTypes = new GrantTypesModel(context)
                {
                    Client = new Client {Id = clientId}
                };
                post = await grantTypes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.AllowedGrantTypes)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);

                Assert.Empty(client.AllowedGrantTypes);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/GrantTypes", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(grantTypes.Client.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var grantTypes = new GrantTypesModel(context.Object)
            {
                Client = new Client {Id = 0}
            };

            // Act
            var post = await grantTypes.OnPostAsync().ConfigureAwait(false);

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
                var grantTypes = new GrantTypesModel(context)
                {
                    Client = new Client {Id = Random.Next()}
                };
                post = await grantTypes.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
