namespace Authentication.Api.Tests.Users
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
    using Pages.Users;
    using Xunit;

    public class DeleteFacts
    {
        private readonly Mock<UserManager<User>> _userManager;

        public DeleteFacts()
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
            var delete = new DeleteModel(_userManager.Object);

            // Act
            var get = await delete.OnGetAsync(user.Id).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{user.Id}"), Times.Once);
            Assert.Equal(user, delete.UserModel);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var delete = new DeleteModel(_userManager.Object);
            var id = Guid.Empty;

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Never);
            Assert.Null(delete.UserModel);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var delete = new DeleteModel(_userManager.Object);
            var id = Guid.NewGuid();

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            Assert.Null(delete.UserModel);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var delete = new DeleteModel(_userManager.Object) {UserModel = user};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{user.Id}"), Times.Once);
            _userManager.Verify(x => x.DeleteAsync(user), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var id = Guid.Empty;
            var delete = new DeleteModel(_userManager.Object)
            {
                UserModel = new User {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Never);
            _userManager.Verify(x => x.DeleteAsync(user), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var id = Guid.NewGuid();
            var delete = new DeleteModel(_userManager.Object)
            {
                UserModel = new User {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            _userManager.Verify(x => x.DeleteAsync(user), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
