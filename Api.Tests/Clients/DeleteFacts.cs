namespace Authentication.Api.Tests.Clients
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.Clients;
    using Xunit;

    public class DeleteFacts
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
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(client.Id).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Once);
            Assert.Equal(client, delete.Client);
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
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(0).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Never);
            Assert.Null(delete.Client);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync(client);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var delete = new DeleteModel(context.Object);
            var id = Random.Next();

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(id), Times.Once);
            Assert.Null(delete.Client);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync(client);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var delete = new DeleteModel(context.Object) {Client = client};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Once);
            clients.Verify(x => x.Remove(client), Times.Once);
            context.Verify(x => x.SaveChangesAsync(), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync(client);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var delete = new DeleteModel(context.Object)
            {
                Client = new Client {Id = 0}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Never);
            clients.Verify(x => x.Remove(client), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var client = new Client {Id = Random.Next()};
            var clients = new Mock<DbSet<Client>>();
            clients.Setup(x => x.FindAsync(client.Id)).ReturnsAsync((Client)null);
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var delete = new DeleteModel(context.Object) {Client = client};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            clients.Verify(x => x.FindAsync(client.Id), Times.Once);
            clients.Verify(x => x.Remove(client), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
