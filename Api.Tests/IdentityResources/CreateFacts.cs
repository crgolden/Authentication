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
            Assert.NotNull(create.IdentityResource);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var identityResources = new Mock<DbSet<IdentityResource>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.IdentityResources).Returns(identityResources.Object);
            var identityResource = new IdentityResource
            {
                Name = "Name",
                Id = new Random().Next()
            };
            var create = new CreateModel(context.Object) {IdentityResource = identityResource};

            // Act
            var post = await create.OnPostAsync().ConfigureAwait(false);

            // Assert
            identityResources.Verify(
                x => x.Add(It.Is<IdentityResource>(y => y.Name.Equals(identityResource.Name))),
                Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(IdentityResource.Id), key);
                Assert.Equal(create.IdentityResource.Id, value);
            });
        }
    }
}
