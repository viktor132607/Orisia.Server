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
        IQueryable<Event> query = PublishedQuery();

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

    public async Task<IEnumerable<Event>> GetUpcomingAsync(
        DateTime from,
        EventType? type = null,
        int take = 10)
    {
        IQueryable<Event> query = PublishedQuery()
            .Where(item =>
                item.EndAt.HasValue
                    ? item.EndAt.Value >= from
                    : item.StartAt >= from);

        if (type.HasValue)
        {
            query = query.Where(item => item.EventType == type.Value);
        }

        return await query
            .OrderBy(item => item.StartAt)
            .ThenBy(item => item.TitleBg)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetPastAsync(
        DateTime before,
        EventType? type = null,
        int take = 10)
    {
        IQueryable<Event> query = PublishedQuery()
            .Where(item =>
                item.EndAt.HasValue
                    ? item.EndAt.Value < before
                    : item.StartAt < before);

        if (type.HasValue)
        {
            query = query.Where(item => item.EventType == type.Value);
        }

        return await query
            .OrderByDescending(item => item.EndAt ?? item.StartAt)
            .ThenByDescending(item => item.StartAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetForAdminAsync(
        EventType? type = null,
        PublicationStatus? status = null)
    {
        IQueryable<Event> query = Context.Events
            .AsNoTracking()
            .Where(item => !item.IsDeleted);

        if (type.HasValue)
        {
            query = query.Where(item => item.EventType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(item => item.Status == status.Value);
        }

        return await query
            .OrderByDescending(item => item.ModifiedOn)
            .ThenByDescending(item => item.StartAt)
            .ToListAsync();
    }

    private IQueryable<Event> PublishedQuery()
    {
        return Context.Events
            .AsNoTracking()
            .Where(item =>
                !item.IsDeleted
                && item.Status == PublicationStatus.Published);
    }
}
