using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class GalleryRepository(ApplicationDbContext context) : IGalleryRepository
{
    public async Task<IEnumerable<GalleryAlbum>> GetAlbumsAsync(
        bool publicOnly,
        bool? featured = null)
    {
        IQueryable<GalleryAlbum> query = AlbumQuery();

        if (publicOnly)
        {
            query = query.Where(album => album.Active);
        }

        if (featured.HasValue)
        {
            query = query.Where(album => album.Featured == featured.Value);
        }

        return await query
            .OrderBy(album => album.SortOrder)
            .ThenByDescending(album => album.CreatedOn)
            .ToListAsync();
    }

    public async Task<GalleryAlbum?> GetAlbumBySlugAsync(string slug, bool publicOnly)
    {
        IQueryable<GalleryAlbum> query = AlbumQuery()
            .Where(album => album.Slug == slug);

        if (publicOnly)
        {
            query = query.Where(album => album.Active);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<GalleryAlbum?> GetAlbumByIdAsync(Guid id, bool tracking = false)
    {
        IQueryable<GalleryAlbum> query = tracking
            ? context.GalleryAlbums
            : context.GalleryAlbums.AsNoTracking();

        return await query
            .Include(album => album.CoverMedia)
            .Include(album => album.Items.Where(item => !item.IsDeleted))
                .ThenInclude(item => item.Media)
            .FirstOrDefaultAsync(album => album.Id == id && !album.IsDeleted);
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingAlbumId = null)
    {
        return context.GalleryAlbums.AnyAsync(album =>
            album.Slug == slug
            && !album.IsDeleted
            && (!excludingAlbumId.HasValue || album.Id != excludingAlbumId.Value));
    }

    public async Task<GalleryAlbum> AddAlbumAsync(GalleryAlbum album)
    {
        context.GalleryAlbums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    public async Task<GalleryAlbum?> UpdateAlbumAsync(GalleryAlbum album)
    {
        GalleryAlbum? current = await context.GalleryAlbums
            .FirstOrDefaultAsync(item => item.Id == album.Id && !item.IsDeleted);

        if (current is null)
        {
            return null;
        }

        DateTime createdOn = current.CreatedOn;
        context.Entry(current).CurrentValues.SetValues(album);
        context.Entry(current).Property(nameof(GenericEntity.CreatedOn)).CurrentValue = createdOn;
        current.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return current;
    }

    public async Task<bool> DeleteAlbumAsync(Guid id)
    {
        GalleryAlbum? album = await context.GalleryAlbums
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);

        if (album is null)
        {
            return false;
        }

        album.IsDeleted = true;
        album.ModifiedOn = DateTime.UtcNow;

        foreach (GalleryMedia item in album.Items.Where(item => !item.IsDeleted))
        {
            item.IsDeleted = true;
            item.ModifiedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public Task<bool> MediaLinkExistsAsync(Guid albumId, Guid mediaId)
    {
        return context.GalleryMedia.AnyAsync(item =>
            item.GalleryAlbumId == albumId
            && item.MediaId == mediaId
            && !item.IsDeleted);
    }

    public async Task<IReadOnlyCollection<GalleryMedia>> AddMediaAsync(IEnumerable<GalleryMedia> items)
    {
        GalleryMedia[] values = items.ToArray();
        context.GalleryMedia.AddRange(values);
        await context.SaveChangesAsync();
        return values;
    }

    public async Task<GalleryMedia?> GetGalleryMediaAsync(Guid id, bool tracking = false)
    {
        IQueryable<GalleryMedia> query = tracking
            ? context.GalleryMedia
            : context.GalleryMedia.AsNoTracking();

        return await query
            .Include(item => item.Media)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public async Task<GalleryMedia?> UpdateGalleryMediaAsync(GalleryMedia item)
    {
        GalleryMedia? current = await context.GalleryMedia
            .FirstOrDefaultAsync(value => value.Id == item.Id && !value.IsDeleted);

        if (current is null)
        {
            return null;
        }

        DateTime createdOn = current.CreatedOn;
        context.Entry(current).CurrentValues.SetValues(item);
        context.Entry(current).Property(nameof(GenericEntity.CreatedOn)).CurrentValue = createdOn;
        current.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return current;
    }

    public async Task<bool> DeleteGalleryMediaAsync(Guid id)
    {
        GalleryMedia? item = await context.GalleryMedia
            .FirstOrDefaultAsync(value => value.Id == id && !value.IsDeleted);

        if (item is null)
        {
            return false;
        }

        item.IsDeleted = true;
        item.ModifiedOn = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task ReorderAsync(Guid albumId, IReadOnlyDictionary<Guid, int> order)
    {
        List<GalleryMedia> items = await context.GalleryMedia
            .Where(item =>
                item.GalleryAlbumId == albumId
                && !item.IsDeleted
                && order.Keys.Contains(item.Id))
            .ToListAsync();

        if (items.Count != order.Count)
        {
            throw new InvalidOperationException("One or more gallery media items do not belong to this album.");
        }

        foreach (GalleryMedia item in items)
        {
            item.SortOrder = order[item.Id];
            item.ModifiedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task<int> GetNextSortOrderAsync(Guid albumId)
    {
        int? max = await context.GalleryMedia
            .Where(item => item.GalleryAlbumId == albumId && !item.IsDeleted)
            .Select(item => (int?)item.SortOrder)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    private IQueryable<GalleryAlbum> AlbumQuery()
    {
        return context.GalleryAlbums
            .AsNoTracking()
            .Where(album => !album.IsDeleted)
            .Include(album => album.CoverMedia)
            .Include(album => album.Items.Where(item => !item.IsDeleted))
                .ThenInclude(item => item.Media);
    }
}
