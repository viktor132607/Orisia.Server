using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IGroupRepository : IRepository<DanceGroup>
{
    Task<IEnumerable<DanceGroup>> GetPublicAsync();
    Task<IEnumerable<DanceGroup>> GetAdminAsync();
    Task<IEnumerable<DanceGroup>> GetActiveForCalendarAsync();
    Task<DanceGroup?> GetBySlugAsync(string slug, bool publicOnly);
    Task<DanceGroup?> GetWithSchedulesAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingGroupId = null);
    Task<DanceGroup?> UpdateWithSchedulesAsync(
        DanceGroup group,
        IReadOnlyCollection<DanceGroupSchedule> schedules);
}
