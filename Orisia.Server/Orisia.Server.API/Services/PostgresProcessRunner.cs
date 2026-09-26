using System.ComponentModel;
using System.Diagnostics;

namespace Orisia.Server.API.Services;

public sealed record PostgresProcessRequest(
    string Executable,
    IReadOnlyCollection<string> Arguments,
    IReadOnlyDictionary<string, string>? Environment,
    string Operation);

public interface IPostgresProcessRunner
{
    Task RunAsync(
        PostgresProcessRequest request,
        CancellationToken cancellationToken);
}

public sealed class PostgresProcessRunner(
    ILogger<PostgresProcessRunner> logger)
    : IPostgresProcessRunner
{
    private const int MaxToolErrorLength = 3000;

    public async Task RunAsync(
        PostgresProcessRequest request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(15));
        ProcessStartInfo startInfo =
            BuildStartInfo(request);

        using Process process =
            new() { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                throw new DatabaseBackupException(
                    $"Unable to start {request.Executable} while trying to {request.Operation}.");
            }
        }
        catch (Win32Exception ex)
        {
            throw new DatabaseBackupException(
                $"The PostgreSQL utility '{request.Executable}' is not installed or is not available in PATH.",
                ex);
        }

        Task standardOutputTask =
            process.StandardOutput.BaseStream.CopyToAsync(Stream.Null);

        Task<string> standardErrorTask =
            process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(
                timeout.Token);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        await standardOutputTask;

        string standardError =
            await standardErrorTask;

        if (process.ExitCode != 0)
        {
            string details =
                NormalizeToolError(standardError);

            logger.LogError(
                "{PostgresTool} failed while trying to {Operation} with exit code {ExitCode}: {ToolError}",
                request.Executable,
                request.Operation,
                process.ExitCode,
                details);

            throw new DatabaseBackupException(
                $"{request.Executable} failed to {request.Operation}. See the server logs for details.");
        }

        if (!string.IsNullOrWhiteSpace(
                standardError))
        {
            logger.LogDebug(
                "{PostgresTool} completed while trying to {Operation}: {ToolOutput}",
                request.Executable,
                request.Operation,
                NormalizeToolError(standardError));
        }

    }

    internal static ProcessStartInfo BuildStartInfo(
        PostgresProcessRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.Executable,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (request.Environment is not null)
        {
            foreach (
                KeyValuePair<string, string> variable
                in request.Environment)
            {
                startInfo.Environment[variable.Key] =
                    variable.Value;
            }
        }

        return startInfo;
    }

    internal static string NormalizeToolError(
        string standardError)
    {
        string details =
            string.IsNullOrWhiteSpace(standardError)
                ? "PostgreSQL did not return additional error details."
                : string.Join(
                    " ",
                    standardError.Split(
                        ['\r', '\n'],
                        StringSplitOptions
                            .RemoveEmptyEntries |
                        StringSplitOptions
                            .TrimEntries));

        if (details.Length > MaxToolErrorLength)
        {
            details =
                details[..MaxToolErrorLength] + "…";
        }

        return details;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(
                    entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cancellation cleanup only.
        }
    }
}
