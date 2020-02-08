namespace Authentication.Api.Tests.Users
{
    using System;
    using System.Collections.Generic;
    using Core.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Pages.Users;
    using Xunit;

    public class IndexFacts
    {
        private readonly Mock<UserManager<User>> _userManager;

        public IndexFacts()
        {
            _userManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>());
        }

        [Fact]
        public void OnGet()
        {
            // Arrange
            var index = new IndexModel(_userManager.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<User>>(index.Users);
            Assert.NotNull(result);
        }
    }
}
