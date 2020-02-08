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

    public class PropertiesFacts
    {
        private static Random Random => new Random();
        private static string DatabaseNamePrefix => typeof(PropertiesFacts).FullName;

        private readonly Mock<IOptionsSnapshot<ConfigurationStoreOptions>> _configurationStoreOptions;
        private readonly Mock<IOptionsSnapshot<OperationalStoreOptions>> _operationalStoreOptions;

        public PropertiesFacts()
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
                Properties = new List<IdentityResourceProperty>
                {
                    new IdentityResourceProperty(),
                    new IdentityResourceProperty(),
                    new IdentityResourceProperty()
                }
            };
            PropertiesModel model;
            IActionResult get;
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                context.Add(identityResource);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new PropertiesModel(context);
                get = await model.OnGetAsync(identityResource.Id).ConfigureAwait(false);
            }

            // Assert
            Assert.NotNull(model.IdentityResource);
            Assert.Equal(identityResource.Id, model.IdentityResource.Id);
            var properties = Assert.IsAssignableFrom<IEnumerable<IdentityResourceProperty>>(model.Properties);
            Assert.Equal(identityResource.Properties.Count, properties.Count());
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var model = new PropertiesModel(new Mock<IConfigurationDbContext>().Object);

            // Act

            var get = await model.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            Assert.Null(model.IdentityResource);
            Assert.Null(model.Properties);
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
            PropertiesModel model;
            IActionResult get;

            // Act
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                model = new PropertiesModel(context);
                get = await model.OnGetAsync(Random.Next()).ConfigureAwait(false);
            }

            // Assert
            Assert.Null(model.IdentityResource);
            Assert.Null(model.Properties);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            const string property1OriginalValue = "Original Value";
            const string property1EditedValue = "Edited Value";
            const string newPropertyValue = "New Value";
            var databaseName = $"{DatabaseNamePrefix}.{nameof(OnPostAsync)}";
            var options = new DbContextOptionsBuilder<IdentityServerDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            PropertiesModel properties;
            IActionResult post;
            var property1 = new IdentityResourceProperty
            {
                Id = Random.Next(),
                Value = property1OriginalValue
            };
            var property2 = new IdentityResourceProperty {Id = Random.Next()};
            var identityResourceId = Random.Next();
            var identityResource = new IdentityResource
            {
                Id = identityResourceId,
                Properties = new List<IdentityResourceProperty>
                {
                    property1,
                    property2
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
                properties = new PropertiesModel(context)
                {
                    IdentityResource = new IdentityResource
                    {
                        Id = identityResourceId,
                        Properties = new List<IdentityResourceProperty>
                        {
                            new IdentityResourceProperty
                            {
                                Id = property1.Id,
                                Value = property1EditedValue
                            },
                            new IdentityResourceProperty {Value = newPropertyValue}
                        }
                    }
                };
                post = await properties.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                identityResource = await context.IdentityResources
                    .Include(x => x.Properties)
                    .SingleOrDefaultAsync(x => x.Id.Equals(identityResourceId))
                    .ConfigureAwait(false);
                property1 = identityResource.Properties.SingleOrDefault(x => x.Id.Equals(property1.Id));
                property2 = identityResource.Properties.SingleOrDefault(x => x.Id.Equals(property2.Id));
                var newProperty = identityResource.Properties.SingleOrDefault(x => x.Value.Equals(newPropertyValue));

                Assert.NotNull(property1);
                Assert.Equal(property1EditedValue, property1.Value);
                Assert.Null(property2);
                Assert.NotNull(newProperty);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Properties", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(properties.IdentityResource.Id, value);
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
            PropertiesModel properties;
            IActionResult post;
            var identityResourceId = Random.Next();
            var identityResource = new IdentityResource
            {
                Id = identityResourceId,
                Properties = new List<IdentityResourceProperty>
                {
                    new IdentityResourceProperty {Id = Random.Next()}
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
                properties = new PropertiesModel(context)
                {
                    IdentityResource = new IdentityResource {Id = identityResourceId}
                };
                post = await properties.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            using (var context = new IdentityServerDbContext(options, _configurationStoreOptions.Object, _operationalStoreOptions.Object))
            {
                identityResource = await context.IdentityResources
                    .Include(x => x.Properties)
                    .SingleOrDefaultAsync(x => x.Id.Equals(identityResourceId))
                    .ConfigureAwait(false);

                Assert.Empty(identityResource.Properties);
            }

            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Properties", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(properties.IdentityResource.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var properties = new PropertiesModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = 0}
            };

            // Act
            var post = await properties.OnPostAsync().ConfigureAwait(false);

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
                var properties = new PropertiesModel(context)
                {
                    IdentityResource = new IdentityResource {Id = Random.Next()}
                };
                post = await properties.OnPostAsync().ConfigureAwait(false);
            }

            // Assert
            Assert.IsType<PageResult>(post);
        }
    }
}
