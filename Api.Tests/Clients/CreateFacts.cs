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

    public class CreateFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var context = new Mock<IConfigurationDbContext>();
            var create = new CreateModel(context.Object);

            // Act
            var get = create.OnGet();

            // Assert
            Assert.NotNull(create.Client);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var clients = new Mock<DbSet<Client>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.Clients).Returns(clients.Object);
            var client = new Client
            {
                ClientName = "ClientName",
                ClientId = "ClientId",
                Id = new Random().Next()
            };
            var create = new CreateModel(context.Object) {Client = client};

            // Act
            var post = await create.OnPostAsync().ConfigureAwait(false);

            // Assert
            clients.Verify(
                x => x.Add(It.Is<Client>(y => y.ClientName.Equals(create.Client.ClientName) &&
                                              y.ClientId.Equals(create.Client.ClientId))),
                Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Client.Id), key);
                Assert.Equal(create.Client.Id, value);
            });
        }
    }
}
