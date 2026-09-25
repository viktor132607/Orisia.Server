using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class SiteReviewRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly SiteReviewRepository _repository;

    public SiteReviewRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("SiteReviewRepositoryTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new SiteReviewRepository(_context);
    }

    [Fact]
    public async Task GetApprovedAsync_ShouldHidePendingRejectedAndDeleted()
    {
        _context.SiteReviews.AddRange(
            CreateReview("approved", ReviewStatus.Approved),
            CreateReview("pending", ReviewStatus.Pending),
            CreateReview("rejected", ReviewStatus.Rejected),
            CreateReview("deleted", ReviewStatus.Approved, isDeleted: true));

        await _context.SaveChangesAsync();

        IEnumerable<SiteReview> result = await _repository.GetApprovedAsync();

        SiteReview only = Assert.Single(result);
        Assert.Equal("approved", only.AuthorName);
    }

    [Fact]
    public async Task GetApprovedAsync_ShouldFilterFeatured()
    {
        _context.SiteReviews.AddRange(
            CreateReview("featured", ReviewStatus.Approved, featured: true),
            CreateReview("normal", ReviewStatus.Approved));

        await _context.SaveChangesAsync();

        IEnumerable<SiteReview> result =
            await _repository.GetApprovedAsync(featured: true);

        SiteReview only = Assert.Single(result);
        Assert.Equal("featured", only.AuthorName);
    }

    private static SiteReview CreateReview(
        string author,
        ReviewStatus status,
        bool featured = false,
        bool isDeleted = false)
    {
        return new SiteReview
        {
            AuthorName = author,
            Content = "Review content",
            Rating = 5,
            Status = status,
            Featured = featured,
            IsDeleted = isDeleted
        };
    }
}
