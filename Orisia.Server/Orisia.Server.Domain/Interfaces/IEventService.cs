using Orisia.Server.Common.Requests.Events;
using Orisia.Server.Common.Responses.Events;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Domain.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventResponse>> GetPublishedAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        EventType? type = null,
        bool? featured = null);

    Task<IEnumerable<EventResponse>> GetUpcomingAsync(EventType? type = null, int take = 10);
    Task<IEnumerable<EventResponse>> GetPastAsync(EventType? type = null, int take = 10);
    Task<EventResponse> GetPublishedBySlugAsync(string slug);

    Task<IEnumerable<EventResponse>> GetAdminAsync(
        EventType? type = null,
        PublicationStatus? status = null);

    Task<EventResponse> GetAdminByIdAsync(Guid id);
    Task<EventResponse> CreateAsync(CreateEventRequest request);
    Task<EventResponse> UpdateAsync(Guid id, UpdateEventRequest request);
    Task<EventResponse> PublishAsync(Guid id);
    Task<EventResponse> UnpublishAsync(Guid id);
    Task<EventResponse> ArchiveAsync(Guid id);
    Task<EventResponse> SetFeaturedAsync(Guid id, bool featured);
    Task<bool> DeleteAsync(Guid id);
}
