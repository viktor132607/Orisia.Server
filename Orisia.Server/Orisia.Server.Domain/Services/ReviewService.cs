using Orisia.Server.Common.Requests.Reviews;
using Orisia.Server.Common.Responses.Reviews;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class ReviewService(
    ISiteReviewRepository reviewRepository,
    IAuthService authService) : IReviewService
{
    public async Task<IEnumerable<SiteReviewResponse>> GetApprovedAsync(
        bool? featured = null,
        int take = 20)
    {
        ValidateTake(take);

        IEnumerable<SiteReview> reviews =
            await reviewRepository.GetApprovedAsync(featured, take);

        return reviews.Select(MapPublic);
    }

    public async Task<SiteReviewResponse> SubmitAsync(SubmitReviewRequest request)
    {
        SiteReview review = new()
        {
            AuthorName = request.AuthorName.Trim(),
            Content = request.Content.Trim(),
            Rating = request.Rating,
            Status = ReviewStatus.Pending,
            Featured = false,
            UserId = await GetOptionalCurrentUserIdAsync()
        };

        SiteReview? created = await reviewRepository.AddAsync(review);
        if (created is null)
        {
            throw new AppException("Review could not be submitted.").SetStatusCode(500);
        }

        return MapPublic(created);
    }

    public async Task<IEnumerable<SiteReviewResponse>> GetAdminAsync(
        ReviewStatus? status = null)
    {
        IEnumerable<SiteReview> reviews =
            await reviewRepository.GetForAdminAsync(status);

        return reviews.Select(MapAdmin);
    }

    public async Task<SiteReviewResponse> GetAdminByIdAsync(Guid id)
    {
        SiteReview? review = await reviewRepository.GetWithModerationAsync(id);
        if (review is null)
        {
            throw new AppException("Review not found.").SetStatusCode(404);
        }

        return MapAdmin(review);
    }

    public Task<SiteReviewResponse> ApproveAsync(Guid id)
    {
        return ModerateAsync(id, ReviewStatus.Approved);
    }

    public Task<SiteReviewResponse> RejectAsync(Guid id)
    {
        return ModerateAsync(id, ReviewStatus.Rejected);
    }

    public async Task<SiteReviewResponse> SetFeaturedAsync(Guid id, bool featured)
    {
        SiteReview? existing = await reviewRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Review not found.").SetStatusCode(404);
        }

        if (featured && existing.Status != ReviewStatus.Approved)
        {
            throw new AppException("Only approved reviews can be featured.").SetStatusCode(409);
        }

        SiteReview updated = Clone(existing);
        updated.Featured = featured;

        SiteReview? saved = await reviewRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Review not found.").SetStatusCode(404);
        }

        return await GetAdminByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await GetAdminByIdAsync(id);
        return await reviewRepository.DeleteAsync(id);
    }

    private async Task<SiteReviewResponse> ModerateAsync(
        Guid id,
        ReviewStatus status)
    {
        SiteReview? existing = await reviewRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Review not found.").SetStatusCode(404);
        }

        Guid moderatorId = await GetRequiredCurrentUserIdAsync();

        SiteReview updated = Clone(existing);
        updated.Status = status;
        updated.ModeratedAt = DateTime.UtcNow;
        updated.ModeratedById = moderatorId;

        if (status != ReviewStatus.Approved)
        {
            updated.Featured = false;
        }

        SiteReview? saved = await reviewRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Review not found.").SetStatusCode(404);
        }

        return await GetAdminByIdAsync(id);
    }

    private static SiteReview Clone(SiteReview review)
    {
        return new SiteReview
        {
            Id = review.Id,
            AuthorName = review.AuthorName,
            UserId = review.UserId,
            Content = review.Content,
            Rating = review.Rating,
            Status = review.Status,
            Featured = review.Featured,
            ModeratedAt = review.ModeratedAt,
            ModeratedById = review.ModeratedById
        };
    }

    private async Task<Guid?> GetOptionalCurrentUserIdAsync()
    {
        string? value = await authService.GetCurrentUserId();
        return Guid.TryParse(value, out Guid userId) ? userId : null;
    }

    private async Task<Guid> GetRequiredCurrentUserIdAsync()
    {
        Guid? id = await GetOptionalCurrentUserIdAsync();
        if (!id.HasValue)
        {
            throw new AppException("Unauthorized.").SetStatusCode(401);
        }

        return id.Value;
    }

    private static SiteReviewResponse MapPublic(SiteReview review)
    {
        return new SiteReviewResponse
        {
            Id = review.Id,
            AuthorName = review.AuthorName,
            UserId = review.UserId,
            Content = review.Content,
            Rating = review.Rating,
            Status = review.Status,
            Featured = review.Featured,
            CreatedOn = review.CreatedOn,
            ModifiedOn = review.ModifiedOn,
            ModeratedAt = null,
            ModeratedById = null,
            ModeratedByName = null
        };
    }

    private static SiteReviewResponse MapAdmin(SiteReview review)
    {
        return new SiteReviewResponse
        {
            Id = review.Id,
            AuthorName = review.AuthorName,
            UserId = review.UserId,
            Content = review.Content,
            Rating = review.Rating,
            Status = review.Status,
            Featured = review.Featured,
            CreatedOn = review.CreatedOn,
            ModifiedOn = review.ModifiedOn,
            ModeratedAt = review.ModeratedAt,
            ModeratedById = review.ModeratedById,
            ModeratedByName = review.ModeratedBy?.Names
        };
    }

    private static void ValidateTake(int take)
    {
        if (take is < 1 or > 50)
        {
            throw new AppException("Take must be between 1 and 50.").SetStatusCode(400);
        }
    }
}
