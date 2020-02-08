namespace Authentication.Api.Tests.Users.Edit
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
    using Pages.Users.Edit;
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

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = "Email"
            };
            var user2 = new User
            {
                Id = user1.Id,
                Email = "New Email"
            };
            _userManager.Setup(x => x.FindByIdAsync($"{user1.Id}")).ReturnsAsync(user1);
            var index = new IndexModel(_userManager.Object) {UserModel = user2};

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(
                x => x.UpdateAsync(It.Is<User>(y => y.Id.Equals(user1.Id) && y.Email.Equals(user2.Email))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("../Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(User.Id), key);
                Assert.Equal(user1.Id, value);
            });
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var id = Guid.Empty;
            var index = new IndexModel(_userManager.Object)
            {
                UserModel = new User {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var id = Guid.NewGuid();
            var index = new IndexModel(_userManager.Object)
            {
                UserModel = new User {Id = id}
            };

            // Act
            var post = await index.OnPostAsync().ConfigureAwait(false);

            // Assert
            _userManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            Assert.IsType<PageResult>(post);
        }
    }
}
