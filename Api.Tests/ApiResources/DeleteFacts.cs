namespace Authentication.Api.Tests.ApiResources
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.ApiResources;
    using Xunit;

    public class DeleteFacts
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
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(apiResource.Id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(apiResource.Id), Times.Once);
            Assert.Equal(apiResource, delete.ApiResource);
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
            var delete = new DeleteModel(context.Object);
            const int id = 0;

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Never);
            Assert.Null(delete.ApiResource);
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
            var delete = new DeleteModel(context.Object);
            var id = Random.Next();

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(delete.ApiResource);
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
            var delete = new DeleteModel(context.Object) {ApiResource = apiResource};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(apiResource.Id), Times.Once);
            apiResources.Verify(x => x.Remove(apiResource), Times.Once);
            context.Verify(x => x.SaveChangesAsync(), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
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
            var delete = new DeleteModel(context.Object)
            {
                ApiResource = new ApiResource {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Never);
            apiResources.Verify(x => x.Remove(apiResource), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
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
            var delete = new DeleteModel(context.Object)
            {
                ApiResource = new ApiResource {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(x => x.FindAsync(id), Times.Once);
            apiResources.Verify(x => x.Remove(apiResource), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
