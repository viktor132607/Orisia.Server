using Npgsql;

namespace Orisia.Server.API.Services;

public interface IPostgresBackupTool
{
    Task CreateBackupAsync(
        string backupPath,
        CancellationToken cancellationToken);

    Task ValidateArchiveAsync(
        string backupPath,
        CancellationToken cancellationToken);

    Task RestoreBackupAsync(
        string backupPath,
        CancellationToken cancellationToken);
}

public sealed class PostgresBackupTool(
    DatabaseBackupConnection connection,
    IPostgresProcessRunner processRunner,
    IPostgresExecutableResolver? executableResolver = null)
    : IPostgresBackupTool
{
    public async Task CreateBackupAsync(
        string backupPath,
        CancellationToken cancellationToken) =>
        await processRunner.RunAsync(
            new PostgresProcessRequest(
                await ResolveAsync("pg_dump", cancellationToken),
                [
                    "--no-password",
                    "--format=custom",
                    "--compress=9",
                    "--file",
                    backupPath,
                    "--dbname",
                    connection.Database
                ],
                connection.CreateEnvironment(),
                "create the database backup"),
            cancellationToken);

    public async Task ValidateArchiveAsync(
        string backupPath,
        CancellationToken cancellationToken) =>
        await processRunner.RunAsync(
            new PostgresProcessRequest(
                await ResolveAsync("pg_restore", cancellationToken),
                ["--list", backupPath],
                null,
                "validate the uploaded database backup"),
            cancellationToken);

    public async Task RestoreBackupAsync(
        string backupPath,
        CancellationToken cancellationToken)
    {
        NpgsqlConnection.ClearAllPools();

        await processRunner.RunAsync(
            new PostgresProcessRequest(
                await ResolveAsync("pg_restore", cancellationToken),
                [
                    "--no-password",
                    "--clean",
                    "--if-exists",
                    "--no-owner",
                    "--no-privileges",
                    "--single-transaction",
                    "--exit-on-error",
                    "--dbname",
                    connection.Database,
                    backupPath
                ],
                connection.CreateEnvironment(),
                "restore the database backup"),
            cancellationToken);

        NpgsqlConnection.ClearAllPools();
    }

    private Task<string> ResolveAsync(string executable, CancellationToken cancellationToken) =>
        executableResolver is null
            ? Task.FromResult(executable)
            : executableResolver.ResolveAsync(executable, cancellationToken);
}
