namespace Authentication.Api.Tests.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Pages.Roles;
    using Xunit;

    public class DeleteFacts
    {
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<UserManager<User>> _userManager;

        public DeleteFacts()
        {
            _roleManager = new Mock<RoleManager<Role>>(
                Mock.Of<IRoleStore<Role>>(),
                new List<IRoleValidator<Role>>(),
                Mock.Of<ILookupNormalizer>(),
                new IdentityErrorDescriber(),
                Mock.Of<ILogger<RoleManager<Role>>>());
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
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object);

            // Act
            var get = await delete.OnGetAsync(role.Id).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{role.Id}"), Times.Once);
            Assert.Equal(role, delete.Role);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidId()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object);
            var id = Guid.Empty;

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Never);
            Assert.Null(delete.Role);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object);
            var id = Guid.NewGuid();

            // Act
            var get = await delete.OnGetAsync(id).ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            Assert.Null(delete.Role);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var users = new List<User> {new User()};
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Name"
            };
            users.ForEach(x => role.UserRoles.Add(new UserRole { User = x }));
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            _userManager.Setup(x => x.GetUsersInRoleAsync(role.Name)).ReturnsAsync(users);
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object) {Role = role};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{role.Id}"), Times.Once);
            _userManager.Verify(x => x.GetUsersInRoleAsync(role.Name), Times.Once);
            foreach (var user in role.UserRoles.Select(x => x.User))
            {
                _userManager.Verify(
                    x => x.RemoveClaimAsync(
                        user,
                        It.Is<Claim>(y => y.Type.Equals(ClaimTypes.Role) && y.Value.Equals(role.Name))),
                    Times.Once);
            }
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidId()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var id = Guid.Empty;
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object)
            {
                Role = new Role {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Never);
            _roleManager.Verify(x => x.DeleteAsync(role), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var role = new Role {Id = Guid.NewGuid()};
            _roleManager.Setup(x => x.FindByIdAsync($"{role.Id}")).ReturnsAsync(role);
            var id = Guid.NewGuid();
            var delete = new DeleteModel(_roleManager.Object, _userManager.Object)
            {
                Role = new Role {Id = id}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            _roleManager.Verify(x => x.FindByIdAsync($"{id}"), Times.Once);
            _roleManager.Verify(x => x.DeleteAsync(role), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
