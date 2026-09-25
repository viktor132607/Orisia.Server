using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IMediaRepository : IRepository<Media>
{
    Task<Media?> GetWithUploaderAsync(Guid id);
    Task<IEnumerable<Media>> GetAllWithUploaderAsync();
    Task<bool> IsInUseAsync(Guid id);
}
