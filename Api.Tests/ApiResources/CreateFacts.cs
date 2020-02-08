namespace Authentication.Api.Tests.ApiResources
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.ApiResources;
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
            Assert.NotNull(create.ApiResource);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var apiResources = new Mock<DbSet<ApiResource>>();
            var context = new Mock<IConfigurationDbContext>();
            context.Setup(x => x.ApiResources).Returns(apiResources.Object);
            var apiResource = new ApiResource
            {
                Name = "Name",
                Id = new Random().Next()
            };
            var create = new CreateModel(context.Object)
            {
                ApiResource = apiResource
            };

            // Act
            var post = await create.OnPostAsync().ConfigureAwait(false);

            // Assert
            apiResources.Verify(
                x => x.Add(It.Is<ApiResource>(y => y.Name.Equals(apiResource.Name))),
                Times.Once);
            context.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(ApiResource.Id), key);
                Assert.Equal(create.ApiResource.Id, value);
            });
        }
    }
}
