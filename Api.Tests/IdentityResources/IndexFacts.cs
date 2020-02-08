namespace Authentication.Api.Tests.IdentityResources
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.IdentityResources;
    using Xunit;

    public class IndexFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var identityResources = new Mock<DbSet<IdentityResource>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var index = new IndexModel(context.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<IdentityResource>>(index.IdentityResources);
            Assert.NotNull(result);
        }
    }
}
