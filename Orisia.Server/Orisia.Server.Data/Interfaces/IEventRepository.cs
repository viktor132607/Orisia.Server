using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingEventId = null);

    Task<IEnumerable<Event>> GetPublishedAsync(
        DateTime? from = null,
        DateTime? to = null,
        EventType? type = null,
        bool? featured = null);

    Task<IEnumerable<Event>> GetUpcomingAsync(
        DateTime from,
        EventType? type = null,
        int take = 10);

    Task<IEnumerable<Event>> GetPastAsync(
        DateTime before,
        EventType? type = null,
        int take = 10);

    Task<IEnumerable<Event>> GetForAdminAsync(
        EventType? type = null,
        PublicationStatus? status = null);

    Task<IEnumerable<Event>> GetCalendarCandidatesAsync(
        DateTime from,
        DateTime to);
}
