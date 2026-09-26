using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class GroupRepository(ApplicationDbContext context)
    : Repository<DanceGroup>(context), IGroupRepository
{
    private IQueryable<DanceGroup> WithSchedules()
    {
        return Context.DanceGroups
            .AsNoTracking()
            .Include(item => item.Schedules.Where(schedule => !schedule.IsDeleted))
            .Where(item => !item.IsDeleted);
    }

    public async Task<IEnumerable<DanceGroup>> GetPublicAsync()
    {
        return await WithSchedules()
            .Where(item => item.Active)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.NameBg)
            .ToListAsync();
    }

    public async Task<IEnumerable<DanceGroup>> GetAdminAsync()
    {
        return await WithSchedules()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.NameBg)
            .ToListAsync();
    }

    public async Task<IEnumerable<DanceGroup>> GetActiveForCalendarAsync()
    {
        return await WithSchedules()
            .Where(item => item.Active)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.NameBg)
            .ToListAsync();
    }

    public async Task<DanceGroup?> GetBySlugAsync(string slug, bool publicOnly)
    {
        IQueryable<DanceGroup> query = WithSchedules().Where(item => item.Slug == slug);
        if (publicOnly) query = query.Where(item => item.Active);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<DanceGroup?> GetWithSchedulesAsync(Guid id)
    {
        return await WithSchedules().FirstOrDefaultAsync(item => item.Id == id);
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingGroupId = null)
    {
        return Context.DanceGroups.AnyAsync(item =>
            item.Slug == slug
            && !item.IsDeleted
            && (!excludingGroupId.HasValue || item.Id != excludingGroupId.Value));
    }

    public async Task<DanceGroup?> UpdateWithSchedulesAsync(
        DanceGroup group,
        IReadOnlyCollection<DanceGroupSchedule> schedules)
    {
        DanceGroup? current = await Context.DanceGroups
            .Include(item => item.Schedules)
            .FirstOrDefaultAsync(item => item.Id == group.Id && !item.IsDeleted);

        if (current is null) return null;

        current.Slug = group.Slug;
        current.NameBg = group.NameBg;
        current.NameEn = group.NameEn;
        current.DescriptionBg = group.DescriptionBg;
        current.DescriptionEn = group.DescriptionEn;
        current.Location = group.Location;
        current.Active = group.Active;
        current.SortOrder = group.SortOrder;
        current.ModifiedOn = DateTime.UtcNow;

        Context.DanceGroupSchedules.RemoveRange(current.Schedules);
        current.Schedules.Clear();

        foreach (DanceGroupSchedule schedule in schedules)
        {
            schedule.DanceGroupId = current.Id;
            current.Schedules.Add(schedule);
        }

        await Context.SaveChangesAsync();
        return await GetWithSchedulesAsync(current.Id);
    }
}
