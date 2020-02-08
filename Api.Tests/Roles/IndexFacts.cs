namespace Authentication.Api.Tests.Roles
{
    using System.Collections.Generic;
    using Core.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Pages.Roles;
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
        public void OnGet()
        {
            // Arrange
            var index = new IndexModel(_roleManager.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<Role>>(index.Roles);
            Assert.NotNull(result);
        }
    }
}
