using Orisia.Server.Common.Requests.Reviews;
using Orisia.Server.Common.Responses.Reviews;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Domain.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<SiteReviewResponse>> GetApprovedAsync(
        bool? featured = null,
        int take = 20);

    Task<SiteReviewResponse> SubmitAsync(SubmitReviewRequest request);

    Task<IEnumerable<SiteReviewResponse>> GetAdminAsync(
        ReviewStatus? status = null);

    Task<SiteReviewResponse> GetAdminByIdAsync(Guid id);
    Task<SiteReviewResponse> ApproveAsync(Guid id);
    Task<SiteReviewResponse> RejectAsync(Guid id);
    Task<SiteReviewResponse> SetFeaturedAsync(Guid id, bool featured);
    Task<bool> DeleteAsync(Guid id);
}
