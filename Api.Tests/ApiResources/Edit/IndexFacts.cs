namespace Authentication.Api.Tests.ApiResources.Edit
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.ApiResources.Edit;
    using Xunit;

    public class IndexFacts
    {
        private static Random Random => new Random();

        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(apiResource.Id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(apiResource.Id), Times.Once);
            Assert.Equal(apiResource, index.ApiResource);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            const int id = 0;
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Never);
            Assert.Null(index.ApiResource);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var index = new IndexModel(context.Object);
            var id = Random.Next();

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(index.ApiResource);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var index = new IndexModel(context.Object)
            {
                ApiResource = new ApiResource {Id = apiResource.Id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(apiResource.Id), Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(ApiResource.Id), key);
                Assert.Equal(index.ApiResource.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            const int id = 0;
            var index = new IndexModel(context.Object)
            {
                ApiResource = new ApiResource {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Never);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var apiResource = new ApiResource {Id = Random.Next()};
            var apiResources = new Mock<DbSet<ApiResource>>();
            apiResources.Setup(x => x.FindAsync(apiResource.Id)).ReturnsAsync(apiResource);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var id = Random.Next();
            var index = new IndexModel(context.Object)
            {
                ApiResource = new ApiResource {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
