using Orisia.Server.Common.Requests.Review;
using Orisia.Server.Common.Responses.Review;
using Orisia.Server.Core.Pages;

namespace Orisia.Server.Domain.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse?> UpdateAsync(UpdateReviewRequest request);
    Task<ReviewResponse?> CreateAsync(CreateReviewRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<Paginated<ReviewResponse>> SearchReviewsAsync(SearchReviewsRequest request);
}
