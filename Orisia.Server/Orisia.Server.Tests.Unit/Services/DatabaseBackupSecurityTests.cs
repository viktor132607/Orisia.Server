using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Orisia.Server.API.Controllers;
using Orisia.Server.API.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public sealed class DatabaseBackupSecurityTests
{
    [Fact]
    public void BackupEndpointsRequireAdmin()
    {
        var attribute = Assert.Single(typeof(DatabaseBackupController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Admin", attribute.Roles);
        Assert.Empty(typeof(DatabaseBackupController).GetMethods()
            .SelectMany(m => m.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("restore orisia")]
    public async Task RestoreRequiresExactConfirmation(string? confirmation)
    {
        var service = new Mock<IDatabaseBackupService>(MockBehavior.Strict);
        var controller = new DatabaseBackupController(service.Object, NullLogger<DatabaseBackupController>.Instance);
        Assert.IsType<BadRequestObjectResult>(await controller.RestoreAsync(null, default, confirmation));
        service.VerifyNoOtherCalls();
    }
}
