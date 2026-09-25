using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingPostId = null);
    Task<IEnumerable<Post>> GetPublishedAsync(
        PostType? type = null,
        bool? featured = null,
        int? take = null);
}
