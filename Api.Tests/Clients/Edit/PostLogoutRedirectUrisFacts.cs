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

    public class PostLogoutRedirectUrisFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(PostLogoutRedirectUrisFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public PostLogoutRedirectUrisFacts()
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
                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                {
                    new ClientPostLogoutRedirectUri(),
                    new ClientPostLogoutRedirectUri(),
                    new ClientPostLogoutRedirectUri()
                }
            };
            PostLogoutRedirectUrisModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(client);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new PostLogoutRedirectUrisModel(context);
                get = await model.OnGetAsync(client.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.Client);
            Assert.Equal(client.Id, model.Client.Id);
            var postLogoutRedirectUris = Assert.IsAssignableFrom<IEnumerable<ClientPostLogoutRedirectUri>>(model.PostLogoutRedirectUris);
            Assert.Equal(client.PostLogoutRedirectUris.Count, postLogoutRedirectUris.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new PostLogoutRedirectUrisModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.PostLogoutRedirectUris);
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
            PostLogoutRedirectUrisModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new PostLogoutRedirectUrisModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.Client);
            Assert.Null(model.PostLogoutRedirectUris);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string postLogoutRedirectUri1OriginalRedirectUri = "Original PostLogoutRedirectUri";
            const string postLogoutRedirectUri1EditedRedirectUri = "Edited PostLogoutRedirectUri";
            const string newPostLogoutRedirectUriRedirectUri = "New PostLogoutRedirectUri";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            PostLogoutRedirectUrisModel postLogoutRedirectUris;
            IActionResult post;
            var postLogoutRedirectUri1 = new ClientPostLogoutRedirectUri
            {
                Id = Random.Next(),
                PostLogoutRedirectUri = postLogoutRedirectUri1OriginalRedirectUri
            };
            var postLogoutRedirectUri2 = new ClientPostLogoutRedirectUri {Id = Random.Next()};
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                {
                    postLogoutRedirectUri1,
                    postLogoutRedirectUri2
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
                postLogoutRedirectUris = new PostLogoutRedirectUrisModel(context)
                {
                    Client = new Client
                    {
                        Id = clientId,
                        PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                        {
                            new ClientPostLogoutRedirectUri
                            {
                                Id = postLogoutRedirectUri1.Id,
                                PostLogoutRedirectUri = postLogoutRedirectUri1EditedRedirectUri
                            },
                            new ClientPostLogoutRedirectUri {PostLogoutRedirectUri = newPostLogoutRedirectUriRedirectUri}
                        }
                    }
                };
                post = await postLogoutRedirectUris.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.PostLogoutRedirectUris)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);
                postLogoutRedirectUri1 = client.PostLogoutRedirectUris.SingleOrDefault(x => x.Id.Equals(postLogoutRedirectUri1.Id));
                postLogoutRedirectUri2 = client.PostLogoutRedirectUris.SingleOrDefault(x => x.Id.Equals(postLogoutRedirectUri2.Id));
                var newPostLogoutRedirectUri = client.PostLogoutRedirectUris.SingleOrDefault(x => x.PostLogoutRedirectUri.Equals(newPostLogoutRedirectUriRedirectUri));

                Assert.NotNull(postLogoutRedirectUri1);
                Assert.Equal(postLogoutRedirectUri1EditedRedirectUri, postLogoutRedirectUri1.PostLogoutRedirectUri);
                Assert.Null(postLogoutRedirectUri2);
                Assert.NotNull(newPostLogoutRedirectUri);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/PostLogoutRedirectUris", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(postLogoutRedirectUris.Client.Id, value);
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
            PostLogoutRedirectUrisModel postLogoutRedirectUris;
            IActionResult post;
            var clientId = Random.Next();
            var client = new Client
            {
                Id = clientId,
                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                {
                    new ClientPostLogoutRedirectUri {Id = Random.Next()}
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
                postLogoutRedirectUris = new PostLogoutRedirectUrisModel(context)
                {
                    Client = new Client {Id = clientId}
                };
                post = await postLogoutRedirectUris.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                client = await context.Clients
                    .Include(x => x.PostLogoutRedirectUris)
                    .SingleOrDefaultAsync(x => x.Id.Equals(clientId))
                    .ConfigureAwait(false);

                Assert.Empty(client.PostLogoutRedirectUris);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/PostLogoutRedirectUris", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(postLogoutRedirectUris.Client.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var postLogoutRedirectUris = new PostLogoutRedirectUrisModel(context.Object)
            {
                Client = new Client {Id = 0}
            };

            // Act
            var post = await postLogoutRedirectUris.OnPostAsync().ConfigureAwait(false);

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
                var postLogoutRedirectUris = new PostLogoutRedirectUrisModel(context)
                {
                    Client = new Client {Id = Random.Next()}
                };
                post = await postLogoutRedirectUris.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
