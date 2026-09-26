using Npgsql;

namespace Orisia.Server.API.Services;

public interface IPostgresExecutableResolver
{
    Task<string> ResolveAsync(string executable, CancellationToken cancellationToken);
}

/// <summary>Match the client major to the target server for round-trip compatibility.</summary>
public sealed class PostgresExecutableResolver(DatabaseBackupConnection settings) : IPostgresExecutableResolver
{
    public async Task<string> ResolveAsync(string executable, CancellationToken cancellationToken)
    {
        if (executable is not ("pg_dump" or "pg_restore"))
            throw new ArgumentException("Unsupported PostgreSQL executable.", nameof(executable));
        await using var connection = new NpgsqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        int major = connection.PostgreSqlVersion.Major;
        string path = $"/usr/lib/postgresql/{major}/bin/{executable}";
        if (!File.Exists(path))
            throw new DatabaseBackupException($"PostgreSQL {major} client tools are required for this database. Install the matching pg_dump and pg_restore version.");
        return path;
    }
}
