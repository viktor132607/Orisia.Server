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
}
