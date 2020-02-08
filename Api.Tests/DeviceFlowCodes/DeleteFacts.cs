namespace Authentication.Api.Tests.DeviceFlowCodes
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.DeviceFlowCodes;
    using Xunit;

    public class DeleteFacts
    {
        [Fact]
        public async Task OnGetAsync()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(deviceFlowCode);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(deviceFlowCode.UserCode).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Once);
            Assert.Equal(deviceFlowCode, delete.DeviceFlowCode);
            Assert.IsType<PageResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidUserCode()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(deviceFlowCode);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object);

            // Act
            var get = await delete.OnGetAsync(string.Empty).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Never);
            Assert.Null(delete.DeviceFlowCode);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnGetAsync_InvalidModel()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(deviceFlowCode);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object);
            var userCode = $"{Guid.NewGuid()}";

            // Act
            var get = await delete.OnGetAsync(userCode).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(userCode), Times.Once);
            Assert.Null(delete.DeviceFlowCode);
            Assert.IsType<NotFoundResult>(get);
        }

        [Fact]
        public async Task OnPostAsync()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(deviceFlowCode);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object) {DeviceFlowCode = deviceFlowCode};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Once);
            deviceFlowCodes.Verify(x => x.Remove(deviceFlowCode), Times.Once);
            context.Verify(x => x.SaveChangesAsync(), Times.Once);
            var result = Assert.IsType<RedirectToPageResult>(post);
            Assert.Equal("./Index", result.PageName);
        }

        [Fact]
        public async Task OnPostAsync_InvalidUserCode()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(deviceFlowCode);
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object)
            {
                DeviceFlowCode = new DeviceFlowCodes {UserCode = string.Empty}
            };

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Never);
            deviceFlowCodes.Verify(x => x.Remove(deviceFlowCode), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModel()
        {
            // Arrange
            var deviceFlowCode = new DeviceFlowCodes {UserCode = $"{Guid.NewGuid()}"};
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            deviceFlowCodes.Setup(x => x.FindAsync(deviceFlowCode.UserCode)).ReturnsAsync(default(DeviceFlowCodes));
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var delete = new DeleteModel(context.Object) {DeviceFlowCode = deviceFlowCode};

            // Act
            var post = await delete.OnPostAsync().ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Once);
            deviceFlowCodes.Verify(x => x.Remove(deviceFlowCode), Times.Never);
            context.Verify(x => x.SaveChangesAsync(), Times.Never);
            Assert.IsType<PageResult>(post);
        }
    }
}
