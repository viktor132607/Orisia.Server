using Orisia.Server.Common.Requests.Gallery;
using Orisia.Server.Common.Responses.Gallery;

namespace Orisia.Server.Domain.Interfaces;

public interface IGalleryService
{
    Task<IEnumerable<GalleryAlbumResponse>> GetPublicAlbumsAsync(bool? featured = null);
    Task<GalleryAlbumResponse> GetPublicAlbumBySlugAsync(string slug);

    Task<IEnumerable<GalleryAlbumResponse>> GetAdminAlbumsAsync();
    Task<GalleryAlbumResponse> GetAdminAlbumByIdAsync(Guid id);

    Task<GalleryAlbumResponse> CreateAlbumAsync(CreateGalleryAlbumRequest request);
    Task<GalleryAlbumResponse> UpdateAlbumAsync(Guid id, UpdateGalleryAlbumRequest request);
    Task<bool> DeleteAlbumAsync(Guid id);

    Task<IReadOnlyCollection<GalleryMediaResponse>> AddMediaAsync(Guid albumId, AddGalleryMediaRequest request);
    Task<GalleryMediaResponse> UpdateMediaAsync(Guid galleryMediaId, UpdateGalleryMediaRequest request);
    Task<GalleryMediaResponse> MoveMediaAsync(Guid galleryMediaId, MoveGalleryMediaRequest request);
    Task ReorderMediaAsync(Guid albumId, ReorderGalleryMediaRequest request);
    Task<bool> DeleteMediaAsync(Guid galleryMediaId);
}
