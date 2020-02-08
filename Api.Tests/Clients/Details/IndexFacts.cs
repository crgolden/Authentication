namespace Authentication.Api.Tests.Clients.Details
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.Clients.Details;
    using Xunit;

    public class IndexFacts
    {
        private static Random Random => new Random();

        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync(client);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(client.Id).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Once);
            Assert.Equal(client, index.Client);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync(client);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            const int id = 0;
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(id), Times.Never);
            Assert.Null(index.Client);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync((Client)null);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var id = Random.Next();
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(index.Client);
            Assert.IsType<NotFoundResult>(get);
        }
    }
}
