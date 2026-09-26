using Moq;
using Orisia.Server.API.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public sealed class PostgresToolVersionTests
{
    [Fact]
    public async Task DumpValidationAndRestoreUseTheMatchingServerVersion()
    {
        var runner = new Mock<IPostgresProcessRunner>();
        runner.Setup(r => r.RunAsync(It.IsAny<PostgresProcessRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var resolver = new Mock<IPostgresExecutableResolver>();
        resolver.Setup(r => r.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) => $"/usr/lib/postgresql/16/bin/{name}");
        var tool = new PostgresBackupTool(new DatabaseBackupConnection("Host=localhost;Database=test;Username=test"), runner.Object, resolver.Object);
        await tool.CreateBackupAsync("archive.dump", default);
        await tool.ValidateArchiveAsync("archive.dump", default);
        await tool.RestoreBackupAsync("archive.dump", default);
        runner.Verify(r => r.RunAsync(It.Is<PostgresProcessRequest>(p => p.Executable == "/usr/lib/postgresql/16/bin/pg_dump"), It.IsAny<CancellationToken>()), Times.Once);
        runner.Verify(r => r.RunAsync(It.Is<PostgresProcessRequest>(p => p.Executable == "/usr/lib/postgresql/16/bin/pg_restore"), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
