using Npgsql;

namespace Orisia.Server.API.Services;

public sealed class PostgresAdvisoryLock(
    string connectionString,
    ILogger<PostgresAdvisoryLock> logger)
{
    public async Task<IAsyncDisposable> AcquireAsync(long key, CancellationToken cancellationToken = default)
    {
        NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        try
        {
            await using NpgsqlCommand command = new("SELECT pg_advisory_lock(@key);", connection);
            command.Parameters.AddWithValue("key", key);
            await command.ExecuteNonQueryAsync(cancellationToken);
            return new Lease(connection, key, logger);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private sealed class Lease(NpgsqlConnection connection, long key, ILogger logger) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    await using NpgsqlCommand command = new("SELECT pg_advisory_unlock(@key);", connection);
                    command.Parameters.AddWithValue("key", key);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to release PostgreSQL advisory lock {Key}.", key);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}
