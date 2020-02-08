namespace Authentication.Api.Tests.IdentityResources.Edit
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.IdentityResources.Edit;
    using Xunit;

    public class IndexFacts
    {
        private static Random Random => new Random();

        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(identityResource.Id).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Once);
            Assert.Equal(identityResource, index.IdentityResource);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            const int id = 0;
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(id), Times.Never);
            Assert.Null(index.IdentityResource);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var id = Random.Next();
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(index.IdentityResource);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var index = new IndexModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = identityResource.Id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(index.IdentityResource.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            const int id = 0;
            var index = new IndexModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(id), Times.Never);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync(identityResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var id = Random.Next();
            var index = new IndexModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(id), Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
