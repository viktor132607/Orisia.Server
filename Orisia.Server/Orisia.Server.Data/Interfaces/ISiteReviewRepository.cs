using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface ISiteReviewRepository : IRepository<SiteReview>
{
    Task<IEnumerable<SiteReview>> GetApprovedAsync(
        bool? featured = null,
        int take = 20);

    Task<IEnumerable<SiteReview>> GetForAdminAsync(
        ReviewStatus? status = null);

    Task<SiteReview?> GetWithModerationAsync(Guid id);
}
