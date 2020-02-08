namespace Authentication.Api.Tests.DeviceFlowCodes.Details
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.DeviceFlowCodes.Details;
    using Xunit;

    public class IndexFacts
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
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(deviceFlowCode.UserCode).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(deviceFlowCode.UserCode), Times.Once);
            Assert.Equal(deviceFlowCode, index.DeviceFlowCode);
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
            var userCode = string.Empty;
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);

            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(userCode).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(userCode), Times.Never);
            Assert.Null(index.DeviceFlowCode);
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
            var userCode = $"{Guid.NewGuid()}";
            var index = new IndexModel(context.Object);

            // Act
            var get = await index.OnGetAsync(userCode).ConfigureAwait(false);

            // Assert
            deviceFlowCodes.Verify(x => x.FindAsync(userCode), Times.Once);
            Assert.Null(index.DeviceFlowCode);
            Assert.IsType<NotFoundResult>(get);
        }
    }
}
