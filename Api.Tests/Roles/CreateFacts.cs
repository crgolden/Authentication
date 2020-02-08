namespace Authentication.Api.Tests.Roles
{
    using System;
    using System.Collections.Generic;
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

    public class CreateFacts
    {
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<UserManager<User>> _userManager;

        public CreateFacts()
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
        public void OnGet()
        {
            // Arrange
            var create = new CreateModel(_roleManager.Object, _userManager.Object);

            // Act
            var get = create.OnGet();

            // Assert
            Assert.NotNull(create.Role);
            Assert.Empty(create.Users);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var user = new User {Id = Guid.NewGuid()};
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Name"
            };
            role.UserRoles.Add(new UserRole
            {
                User = user,
                UserId = user.Id
            });
            _userManager.Setup(x => x.FindByIdAsync($"{user.Id}")).ReturnsAsync(user);
            var create = new CreateModel(_roleManager.Object, _userManager.Object) {Role = role};

            // Act
            var post = await create.OnPostAsync().ConfigureAwait(false);

            // Assert
            foreach (var userRole in role.UserRoles)
            {
                _userManager.Verify(x => x.FindByIdAsync($"{userRole.UserId}"), Times.Once);
                _userManager.Verify(
                    x => x.AddClaimAsync(userRole.User, It.Is<Claim>(
                        y => y.Type == ClaimTypes.Role && y.Value == role.Name)),
                    Times.Once);
            }
            _roleManager.Verify(
                x => x.CreateAsync(It.Is<Role>(y => y.Name.Equals(role.Name))),
                Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Details/Index", result.PageName);
            Assert.Collection(result.RouteValues, routeValue =>
            {
                var (key, value) = routeValue;
                Assert.Equal(nameof(Role.Id), key);
                Assert.Equal(role.Id, value);
            });
        }
    }
}
