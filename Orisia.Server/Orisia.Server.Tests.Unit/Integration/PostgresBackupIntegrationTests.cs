using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Orisia.Server.API.Services;
using Orisia.Server.Data;
using Orisia.Server.Data.Seed;
using Xunit;

namespace Orisia.Server.Tests.Unit.Integration;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ORISIA_TEST_POSTGRES")))
            Skip = "Requires the isolated CI PostgreSQL service.";
    }
}

public sealed class PostgresBackupIntegrationTests
{
    [PostgresFact]
    public async Task MigrationsAndFullBackupRestorePreserveEveryTableAndRow()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ORISIA_TEST_POSTGRES"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "orisia_ci_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin))
            await create.ExecuteNonQueryAsync();
        settings.Database = database;
        DatabaseBackupArtifact? archive = null;
        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(settings.ConnectionString).Options;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                Assert.Empty(await db.Database.GetPendingMigrationsAsync());
                Assert.False(db.Database.HasPendingModelChanges());
                await UserSeeder.SeedAsync(db);
                await CmsDemoSeeder.SeedAsync(db);
                await CmsDemoSeeder.SeedAsync(db);
                Assert.Equal(1, await db.Posts.CountAsync());
                Assert.Equal(1, await db.Events.CountAsync());
            }

            var before = await SnapshotAsync(settings.ConnectionString);
            using var gate = new DatabaseBackupOperationLock();
            var service = new DatabaseBackupService(
                new PostgresBackupTool(new DatabaseBackupConnection(settings.ConnectionString),
                    new PostgresProcessRunner(NullLogger<PostgresProcessRunner>.Instance)),
                new DatabaseBackupArchiveValidator(), new DatabaseBackupFileStore(), gate,
                NullLogger<DatabaseBackupService>.Instance);
            archive = await service.CreateBackupAsync();
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Posts.ExecuteUpdateAsync(s => s.SetProperty(p => p.TitleBg, "Changed after export"));
                await db.Users.ExecuteUpdateAsync(s => s.SetProperty(u => u.Names, "Changed after export"));
            }
            await using (var stream = File.OpenRead(archive.FilePath))
                await service.RestoreBackupAsync(stream);
            Assert.Equal(before, await SnapshotAsync(settings.ConnectionString));
        }
        finally
        {
            if (archive is not null) File.Delete(archive.FilePath);
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }

    private static async Task<string> SnapshotAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var tables = new List<string>();
        await using (var list = new NpgsqlCommand("SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename", connection))
        await using (var reader = await list.ExecuteReaderAsync())
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
        var snapshot = new SortedDictionary<string, string>();
        foreach (var table in tables)
        {
            var quoted = '"' + table.Replace("\"", "\"\"") + '"';
            await using var command = new NpgsqlCommand(
                $"SELECT COALESCE(jsonb_agg(row ORDER BY row::text), '[]'::jsonb)::text FROM (SELECT to_jsonb(t) AS row FROM public.{quoted} t) rows", connection);
            snapshot.Add(table, (string)(await command.ExecuteScalarAsync())!);
        }
        return JsonSerializer.Serialize(snapshot);
    }
}
