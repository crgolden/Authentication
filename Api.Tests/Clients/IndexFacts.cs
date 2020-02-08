namespace Authentication.Api.Tests.Clients
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.Clients;
    using Xunit;

    public class IndexFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var clients = new Mock<DbSet<Client>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var index = new IndexModel(context.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<Client>>(index.Clients);
            Assert.NotNull(result);
        }
    }
}
