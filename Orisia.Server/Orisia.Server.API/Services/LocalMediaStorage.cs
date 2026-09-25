using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Services;

public class LocalMediaStorage(
    IWebHostEnvironment environment,
    IOptions<MediaStorageOptions> options) : IMediaStorage
{
    private readonly MediaStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        string safeExtension = NormalizeExtension(extension);
        string relativeDirectory = DateTime.UtcNow.ToString("yyyy/MM");
        string key = $"{relativeDirectory}/{Guid.NewGuid():N}{safeExtension}";

        string fullPath = ResolveFullPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using FileStream output = new(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await content.CopyToAsync(output, cancellationToken);
        return key.Replace('\\', '/');
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        string fullPath = ResolveFullPath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string storageKey)
    {
        string basePath = "/" + _options.PublicBasePath.Trim('/');
        string key = storageKey.Replace('\\', '/').TrimStart('/');
        return $"{basePath}/{key}";
    }

    private string ResolveFullPath(string storageKey)
    {
        string root = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);

        root = Path.GetFullPath(root);

        string normalizedKey = storageKey
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        string fullPath = Path.GetFullPath(Path.Combine(root, normalizedKey));
        string rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid media storage key.");
        }

        return fullPath;
    }

    private static string NormalizeExtension(string extension)
    {
        string value = extension.Trim().ToLowerInvariant();
        if (!value.StartsWith('.'))
        {
            value = "." + value;
        }

        if (value.Length > 16 || value.Any(character => !char.IsLetterOrDigit(character) && character != '.'))
        {
            throw new InvalidOperationException("Invalid media file extension.");
        }

        return value;
    }
}
