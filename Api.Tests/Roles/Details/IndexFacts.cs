namespace Authentication.Api.Tests.Roles.Details
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Pages.Roles.Details;
    using Xunit;

    public class IndexFacts
    {
        private readonly Mock<RoleManager<Role>> _roleManager;

        public IndexFacts()
        {
            _roleManager = new Mock<RoleManager<Role>>(
                Mock.Of<IRoleStore<Role>>(),
                new List<IRoleValidator<Role>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<ILogger<RoleManager<Role>>>());
        }

        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var index = new IndexModel(_roleManager.Object);

            // Act
            var get = await index.OnGetAsync(role.Id).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{role.Id}"), Times.Once);
            Assert.Equal(role, index.Role);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var index = new IndexModel(_roleManager.Object);

            // Act
            var get = await index.OnGetAsync(Guid.Empty).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{role.Id}"), Times.Never);
            Assert.Null(index.Role);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var index = new IndexModel(_roleManager.Object);
            var id = Guid.NewGuid();

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            Assert.Null(index.Role);
            Assert.IsType<NotFoundResult>(get);
        }
    }
}
