using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class DanceRepository(ApplicationDbContext context)
    : Repository<Dance>(context), IDanceRepository
{
    public async Task<IEnumerable<Dance>> GetPublicAsync(string? region = null)
    {
        IQueryable<Dance> query = Context.Dances
            .AsNoTracking()
            .Include(item => item.ThumbnailMedia)
            .Where(item => !item.IsDeleted && item.Active);

        if (!string.IsNullOrWhiteSpace(region))
        {
            string normalized = region.Trim();
            query = query.Where(item => item.Region == normalized);
        }

        return await query
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.TitleBg)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dance>> GetAdminAsync()
    {
        return await Context.Dances
            .AsNoTracking()
            .Include(item => item.ThumbnailMedia)
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.TitleBg)
            .ToListAsync();
    }

    public async Task<Dance?> GetBySlugAsync(string slug, bool publicOnly)
    {
        IQueryable<Dance> query = Context.Dances
            .AsNoTracking()
            .Include(item => item.ThumbnailMedia)
            .Where(item => item.Slug == slug && !item.IsDeleted);

        if (publicOnly)
        {
            query = query.Where(item => item.Active);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Dance?> GetWithThumbnailAsync(Guid id)
    {
        return await Context.Dances
            .AsNoTracking()
            .Include(item => item.ThumbnailMedia)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingDanceId = null)
    {
        return Context.Dances.AnyAsync(item =>
            item.Slug == slug
            && !item.IsDeleted
            && (!excludingDanceId.HasValue || item.Id != excludingDanceId.Value));
    }

    public async Task ReorderAsync(IReadOnlyDictionary<Guid, int> order)
    {
        List<Dance> dances = await Context.Dances
            .Where(item => !item.IsDeleted && order.Keys.Contains(item.Id))
            .ToListAsync();

        if (dances.Count != order.Count)
        {
            throw new InvalidOperationException("One or more dances were not found.");
        }

        foreach (Dance dance in dances)
        {
            dance.SortOrder = order[dance.Id];
            dance.ModifiedOn = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync();
    }
}
