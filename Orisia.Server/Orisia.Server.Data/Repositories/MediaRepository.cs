using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class MediaRepository(ApplicationDbContext context)
    : Repository<Media>(context), IMediaRepository
{
    public async Task<Media?> GetWithUploaderAsync(Guid id)
    {
        return await Context.Media
            .AsNoTracking()
            .Include(item => item.UploadedBy)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public async Task<IEnumerable<Media>> GetAllWithUploaderAsync()
    {
        return await Context.Media
            .AsNoTracking()
            .Include(item => item.UploadedBy)
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedOn)
            .ToListAsync();
    }

    public async Task<bool> IsInUseAsync(Guid id)
    {
        bool usedByPost = await Context.Posts.AnyAsync(post =>
            !post.IsDeleted && post.CoverMediaId == id);

        if (usedByPost)
        {
            return true;
        }

        bool usedByEvent = await Context.Events.AnyAsync(item =>
            !item.IsDeleted && item.CoverMediaId == id);

        if (usedByEvent)
        {
            return true;
        }

        bool usedAsAlbumCover = await Context.GalleryAlbums.AnyAsync(album =>
            !album.IsDeleted && album.CoverMediaId == id);

        if (usedAsAlbumCover)
        {
            return true;
        }

        bool usedByGallery = await Context.GalleryMedia.AnyAsync(item =>
            !item.IsDeleted && item.MediaId == id);

        if (usedByGallery)
        {
            return true;
        }

        return await Context.Dances.AnyAsync(item =>
            !item.IsDeleted && item.ThumbnailMediaId == id);
    }
}
