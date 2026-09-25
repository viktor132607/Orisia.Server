using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class EventRepository(ApplicationDbContext context)
    : Repository<Event>(context), IEventRepository
{
    public async Task<Event?> GetBySlugAsync(string slug)
    {
        return await Context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Slug == slug && !item.IsDeleted);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludingEventId = null)
    {
        return await Context.Events.AnyAsync(item =>
            item.Slug == slug
            && !item.IsDeleted
            && (!excludingEventId.HasValue || item.Id != excludingEventId.Value));
    }

    public async Task<IEnumerable<Event>> GetPublishedAsync(
        DateTime? from = null,
        DateTime? to = null,
        EventType? type = null,
        bool? featured = null)
    {
        IQueryable<Event> query = Context.Events
            .AsNoTracking()
            .Where(item =>
                !item.IsDeleted
                && item.Status == PublicationStatus.Published);

        if (from.HasValue)
        {
            query = query.Where(item =>
                item.EndAt.HasValue
                    ? item.EndAt.Value >= from.Value
                    : item.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(item => item.StartAt <= to.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(item => item.EventType == type.Value);
        }

        if (featured.HasValue)
        {
            query = query.Where(item => item.Featured == featured.Value);
        }

        return await query
            .OrderBy(item => item.StartAt)
            .ThenBy(item => item.TitleBg)
            .ToListAsync();
    }
}
