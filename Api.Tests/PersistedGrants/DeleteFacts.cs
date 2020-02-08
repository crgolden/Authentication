namespace Authentication.Api.Tests.PersistedGrants
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.PersistedGrants;
    using Xunit;

    public class DeleteFacts
    {
        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(persistedGrant.Key).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Once);
            Assert.Equal(persistedGrant, delete.PersistedGrant);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidKey()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(string.Empty).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Never);
            Assert.Null(delete.PersistedGrant);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object);
            var key = $"{Guid.NewGuid()}";

            // Act
            var get = await delete.OnGetAsync(key).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(key), Times.Once);
            Assert.Null(delete.PersistedGrant);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object) {PersistedGrant = persistedGrant};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Once);
            persistedGrants.Verify(x => x.Remove(persistedGrant), Times.Once);
            context.Verify(x => x.SaveChangesAsync(), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidKey()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object)
            {
                PersistedGrant = new PersistedGrant {Key = string.Empty}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Never);
            persistedGrants.Verify(x => x.Remove(persistedGrant), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var persistedGrant = new PersistedGrant {Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync((PersistedGrant)null);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var delete = new DeleteModel(context.Object) {PersistedGrant = persistedGrant};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Once);
            persistedGrants.Verify(x => x.Remove(persistedGrant), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
