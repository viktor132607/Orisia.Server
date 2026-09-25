using Orisia.Server.Common.Requests.Posts;
using Orisia.Server.Common.Responses.Posts;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Domain.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostResponse>> GetPublishedAsync(PostType? type = null, bool? featured = null, int? take = null);
    Task<PostResponse> GetPublishedBySlugAsync(string slug);

    Task<IEnumerable<PostResponse>> GetAdminAsync(PostType? type = null, PublicationStatus? status = null);
    Task<PostResponse> GetAdminByIdAsync(Guid id);

    Task<PostResponse> CreateAsync(CreatePostRequest request);
    Task<PostResponse> UpdateAsync(Guid id, UpdatePostRequest request);
    Task<PostResponse> PublishAsync(Guid id);
    Task<PostResponse> ArchiveAsync(Guid id);
    Task<PostResponse> SetFeaturedAsync(Guid id, bool featured);
    Task<bool> DeleteAsync(Guid id);
}
