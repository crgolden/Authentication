namespace Authentication.Api.Tests.ApiResources
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.ApiResources;
    using Xunit;

    public class IndexFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var apiResources = new Mock<DbSet<ApiResource>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var index = new IndexModel(context.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<ApiResource>>(index.ApiResources);
            Assert.NotNull(result);
        }
    }
}
