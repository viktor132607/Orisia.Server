using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IGalleryRepository
{
    Task<IEnumerable<GalleryAlbum>> GetAlbumsAsync(bool publicOnly, bool? featured = null);
    Task<GalleryAlbum?> GetAlbumBySlugAsync(string slug, bool publicOnly);
    Task<GalleryAlbum?> GetAlbumByIdAsync(Guid id, bool tracking = false);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingAlbumId = null);

    Task<GalleryAlbum> AddAlbumAsync(GalleryAlbum album);
    Task<GalleryAlbum?> UpdateAlbumAsync(GalleryAlbum album);
    Task<bool> DeleteAlbumAsync(Guid id);

    Task<bool> MediaLinkExistsAsync(Guid albumId, Guid mediaId);
    Task<IReadOnlyCollection<GalleryMedia>> AddMediaAsync(IEnumerable<GalleryMedia> items);
    Task<GalleryMedia?> GetGalleryMediaAsync(Guid id, bool tracking = false);
    Task<GalleryMedia?> UpdateGalleryMediaAsync(GalleryMedia item);
    Task<bool> DeleteGalleryMediaAsync(Guid id);
    Task ReorderAsync(Guid albumId, IReadOnlyDictionary<Guid, int> order);
    Task<int> GetNextSortOrderAsync(Guid albumId);
}
