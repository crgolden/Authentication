namespace Authentication.Api.Tests.Users.Details
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Pages.Users.Details;
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
        public async Task OnGetAsync()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var index = new IndexModel(_userManager.Object);

            // Act
            var get = await index.OnGetAsync(user.Id).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{user.Id}"), Times.Once);
            Assert.Equal(user, index.UserModel);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var index = new IndexModel(_userManager.Object);

            // Act
            var get = await index.OnGetAsync(Guid.Empty).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{user.Id}"), Times.Never);
            Assert.Null(index.UserModel);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var index = new IndexModel(_userManager.Object);
            var id = Guid.NewGuid();

            // Act
            var get = await index.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            Assert.Null(index.UserModel);
            Assert.IsType<NotFoundResult>(get);
        }
    }
}
