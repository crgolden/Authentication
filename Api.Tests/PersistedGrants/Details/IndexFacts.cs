namespace Authentication.Api.Tests.PersistedGrants.Details
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.PersistedGrants.Details;
    using Xunit;

    public class IndexFacts
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
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(persistedGrant.Key).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(persistedGrant.Key), Times.Once);
            Assert.Equal(persistedGrant, index.PersistedGrant);
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
            var key = string.Empty;
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(key).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(key), Times.Never);
            Assert.Null(index.PersistedGrant);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var persistedGrant = new PersistedGrant { Key = $"{Guid.NewGuid()}"};
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            persistedGrants.Setup(x => x.FindAsync(persistedGrant.Key)).ReturnsAsync(persistedGrant);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var key = $"{Guid.NewGuid()}";
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(key).ConfigureAwait(false);

            // Assert
            persistedGrants.Verify(x => x.FindAsync(key), Times.Once);
            Assert.Null(index.PersistedGrant);
            Assert.IsType<NotFoundResult>(get);
        }
    }
}
