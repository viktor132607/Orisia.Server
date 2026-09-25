using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IDanceRepository : IRepository<Dance>
{
    Task<IEnumerable<Dance>> GetPublicAsync(string? region = null);
    Task<IEnumerable<Dance>> GetAdminAsync();
    Task<Dance?> GetBySlugAsync(string slug, bool publicOnly);
    Task<Dance?> GetWithThumbnailAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingDanceId = null);
    Task ReorderAsync(IReadOnlyDictionary<Guid, int> order);
}
