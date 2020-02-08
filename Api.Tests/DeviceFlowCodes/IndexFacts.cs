namespace Authentication.Api.Tests.DeviceFlowCodes
{
    using System.Collections.Generic;
    using IdentityServer4.EntityFramework.Entities;
    using IdentityServer4.EntityFramework.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Pages.DeviceFlowCodes;
    using Xunit;

    public class IndexFacts
    {
        [Fact]
        public void OnGet()
        {
            // Arrange
            var deviceFlowCodes = new Mock<DbSet<DeviceFlowCodes>>();
            var context = new Mock<IPersistedGrantDbContext>();
            context.Setup(x => x.DeviceFlowCodes).Returns(deviceFlowCodes.Object);
            var index = new IndexModel(context.Object);

            // Act
            index.OnGet();

            // Assert
            var result = Assert.IsAssignableFrom<IEnumerable<DeviceFlowCodes>>(index.DeviceFlowCodes);
            Assert.NotNull(result);
        }
    }
}
