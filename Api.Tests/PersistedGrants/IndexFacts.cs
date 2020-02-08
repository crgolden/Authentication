namespace Authentication.Api.Tests.PersistedGrants
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.PersistedGrants;
    using Xunit;

    public class IndexFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var persistedGrants = new Mock<DbSet<PersistedGrant>>();
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.PersistedGrants).Returns(persistedGrants.Object);
            var index = new IndexModel(context.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<PersistedGrant>>(index.PersistedGrants);
            Assert.NotNull(result);
        }
    }
}
