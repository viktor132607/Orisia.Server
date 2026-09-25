namespace Orisia.Server.Domain.Interfaces;

public interface IMediaStorage
{
    Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string storageKey);
}
