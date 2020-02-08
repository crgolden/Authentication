namespace Authentication.Api.Tests.IdentityResources
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.IdentityResources;
    using Xunit;

    public class DeleteFacts
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
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(identityResource.Id).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Once);
            Assert.Equal(identityResource, delete.IdentityResource);
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
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Never);
            Assert.Null(delete.IdentityResource);
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
            var delete = new DeleteModel(context.Object);
            var id = Random.Next();

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(delete.IdentityResource);
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
            var delete = new DeleteModel(context.Object) {IdentityResource = identityResource};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Once);
            identityResources.Verify(x => x.Remove(identityResource), Times.Once);
            context.Verify(x => x.SaveChangesAsync(), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
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
            var delete = new DeleteModel(context.Object)
            {
                IdentityResource = new IdentityResource {Id = 0}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Never);
            identityResources.Verify(x => x.Remove(identityResource), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var identityResource = new IdentityResource {Id = Random.Next()};
            var identityResources = new Mock<DbSet<IdentityResource>>();
            identityResources.Setup(x => x.FindAsync(identityResource.Id)).ReturnsAsync((IdentityResource)null);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var delete = new DeleteModel(context.Object) {IdentityResource = identityResource};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(x => x.FindAsync(identityResource.Id), Times.Once);
            identityResources.Verify(x => x.Remove(identityResource), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
