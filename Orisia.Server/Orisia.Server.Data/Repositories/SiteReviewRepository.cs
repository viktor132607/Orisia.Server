using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class SiteReviewRepository(ApplicationDbContext context)
    : Repository<SiteReview>(context), ISiteReviewRepository
{
    public async Task<IEnumerable<SiteReview>> GetApprovedAsync(
        bool? featured = null,
        int take = 20)
    {
        IQueryable<SiteReview> query = Context.SiteReviews
            .AsNoTracking()
            .Where(item =>
                !item.IsDeleted
                && item.Status == ReviewStatus.Approved);

        if (featured.HasValue)
        {
            query = query.Where(item => item.Featured == featured.Value);
        }

        return await query
            .OrderByDescending(item => item.Featured)
            .ThenByDescending(item => item.CreatedOn)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<SiteReview>> GetForAdminAsync(
        ReviewStatus? status = null)
    {
        IQueryable<SiteReview> query = Context.SiteReviews
            .AsNoTracking()
            .Include(item => item.ModeratedBy)
            .Where(item => !item.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(item => item.Status == status.Value);
        }

        return await query
            .OrderBy(item => item.Status)
            .ThenByDescending(item => item.CreatedOn)
            .ToListAsync();
    }

    public async Task<SiteReview?> GetWithModerationAsync(Guid id)
    {
        return await Context.SiteReviews
            .AsNoTracking()
            .Include(item => item.ModeratedBy)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }
}
