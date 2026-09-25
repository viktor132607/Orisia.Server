using Moq;
using Orisia.Server.Common.Requests.Reviews;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class ReviewServiceTests
{
    private readonly Mock<ISiteReviewRepository> _reviews = new();
    private readonly Mock<IAuthService> _auth = new();
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _service = new ReviewService(_reviews.Object, _auth.Object);
    }

    [Fact]
    public async Task SubmitAsync_ShouldAlwaysCreatePendingNonFeaturedReview()
    {
        Guid userId = Guid.NewGuid();
        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(userId.ToString());
        _reviews.Setup(x => x.AddAsync(It.IsAny<SiteReview>()))
            .ReturnsAsync((SiteReview review) => review);

        var result = await _service.SubmitAsync(new SubmitReviewRequest
        {
            AuthorName = " Viktor ",
            Content = " Много добро представяне. ",
            Rating = 5
        });

        Assert.Equal(ReviewStatus.Pending, result.Status);
        Assert.False(result.Featured);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Viktor", result.AuthorName);
        Assert.Equal("Много добро представяне.", result.Content);
    }

    [Fact]
    public async Task ApproveAsync_ShouldSetModeratorAndTimestamp()
    {
        Guid id = Guid.NewGuid();
        Guid moderatorId = Guid.NewGuid();
        SiteReview review = CreateReview(id, ReviewStatus.Pending);

        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(moderatorId.ToString());
        _reviews.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(review);
        _reviews.Setup(x => x.UpdateAsync(It.IsAny<SiteReview>()))
            .ReturnsAsync((SiteReview item) => item);
        _reviews.Setup(x => x.GetWithModerationAsync(id))
            .ReturnsAsync((Guid _) =>
            {
                review.Status = ReviewStatus.Approved;
                review.ModeratedById = moderatorId;
                review.ModeratedAt = DateTime.UtcNow;
                return review;
            });

        var result = await _service.ApproveAsync(id);

        Assert.Equal(ReviewStatus.Approved, result.Status);
        Assert.Equal(moderatorId, result.ModeratedById);
        Assert.NotNull(result.ModeratedAt);
    }

    [Fact]
    public async Task RejectAsync_ShouldClearFeaturedFlag()
    {
        Guid id = Guid.NewGuid();
        Guid moderatorId = Guid.NewGuid();
        SiteReview review = CreateReview(id, ReviewStatus.Approved);
        review.Featured = true;

        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(moderatorId.ToString());
        _reviews.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(review);
        _reviews.Setup(x => x.UpdateAsync(It.IsAny<SiteReview>()))
            .ReturnsAsync((SiteReview item) =>
            {
                review.Status = item.Status;
                review.Featured = item.Featured;
                review.ModeratedAt = item.ModeratedAt;
                review.ModeratedById = item.ModeratedById;
                return item;
            });
        _reviews.Setup(x => x.GetWithModerationAsync(id))
            .ReturnsAsync(() => review);

        var result = await _service.RejectAsync(id);

        Assert.Equal(ReviewStatus.Rejected, result.Status);
        Assert.False(result.Featured);
    }

    [Fact]
    public async Task SetFeaturedAsync_ShouldRejectPendingReview()
    {
        Guid id = Guid.NewGuid();
        _reviews.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(CreateReview(id, ReviewStatus.Pending));

        await Assert.ThrowsAsync<AppException>(() =>
            _service.SetFeaturedAsync(id, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task GetApprovedAsync_ShouldRejectInvalidTake(int take)
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _service.GetApprovedAsync(take: take));
    }

    private static SiteReview CreateReview(Guid id, ReviewStatus status)
    {
        return new SiteReview
        {
            Id = id,
            AuthorName = "Author",
            Content = "Review content",
            Rating = 5,
            Status = status
        };
    }
}
