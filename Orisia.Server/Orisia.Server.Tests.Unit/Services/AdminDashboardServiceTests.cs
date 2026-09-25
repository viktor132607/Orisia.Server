using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class AdminDashboardServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AdminDashboardService _service;

    public AdminDashboardServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("AdminDashboardServiceTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _service = new AdminDashboardService(_context);
    }

    [Fact]
    public async Task GetAsync_ShouldAggregateOperationalMetrics()
    {
        DateTime now = DateTime.UtcNow;

        _context.Users.AddRange(
            CreateUser("admin@example.com", Roles.Admin, true),
            CreateUser("editor@example.com", Roles.Editor, true),
            CreateUser("inactive@example.com", Roles.User, false));

        _context.Posts.AddRange(
            CreatePost("published", PublicationStatus.Published),
            CreatePost("draft", PublicationStatus.Draft));

        _context.Events.AddRange(
            CreateEvent("future", now.AddDays(1), PublicationStatus.Published),
            CreateEvent("past", now.AddDays(-2), PublicationStatus.Published),
            CreateEvent("draft-event", now.AddDays(2), PublicationStatus.Draft));

        _context.GalleryAlbums.Add(new GalleryAlbum
        {
            Slug = "album",
            TitleBg = "Албум",
            TitleEn = "Album",
            Active = true,
            Featured = true
        });

        _context.SiteReviews.AddRange(
            new SiteReview
            {
                AuthorName = "Pending",
                Content = "Pending review",
                Rating = 4,
                Status = ReviewStatus.Pending
            },
            new SiteReview
            {
                AuthorName = "Approved",
                Content = "Approved review",
                Rating = 5,
                Status = ReviewStatus.Approved,
                Featured = true
            });

        _context.ContactInquiries.Add(new ContactInquiry
        {
            Name = "Contact",
            Email = "contact@example.com",
            Subject = "Question",
            Message = "Message",
            Status = InquiryStatus.New
        });

        _context.Media.Add(new Media
        {
            OriginalFileName = "photo.jpg",
            StorageKey = "photo.jpg",
            MimeType = "image/jpeg",
            Extension = ".jpg",
            Sha256 = new string('a', 64),
            SizeBytes = 1024
        });

        _context.Dances.AddRange(
            CreateDance("active-dance", true, "Шопска"),
            CreateDance("inactive-dance", false, "Тракийска"));

        await _context.SaveChangesAsync();

        var result = await _service.GetAsync();

        Assert.Equal(3, result.Users.Total);
        Assert.Equal(2, result.Users.Active);
        Assert.Equal(1, result.Users.Inactive);
        Assert.Equal(1, result.Posts.Published);
        Assert.Equal(1, result.Posts.Draft);
        Assert.Equal(1, result.Events.Upcoming);
        Assert.Equal(1, result.Events.Past);
        Assert.Equal(1, result.Reviews.Pending);
        Assert.Equal(1, result.Inquiries.New);
        Assert.Equal(1024, result.Media.TotalSizeBytes);
        Assert.Equal(2, result.Dances.Regions);
        Assert.Single(result.PendingReviews);
        Assert.Single(result.UpcomingEvents);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullAverageWhenNoApprovedReviews()
    {
        var result = await _service.GetAsync();

        Assert.Null(result.Reviews.AverageApprovedRating);
    }

    [Fact]
    public async Task GetAsync_ShouldIgnoreSoftDeletedRecords()
    {
        User deleted = CreateUser("deleted@example.com", Roles.User, true);
        deleted.IsDeleted = true;

        Post deletedPost = CreatePost("deleted-post", PublicationStatus.Published);
        deletedPost.IsDeleted = true;

        _context.AddRange(deleted, deletedPost);
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync();

        Assert.Equal(0, result.Users.Total);
        Assert.Equal(0, result.Posts.Total);
    }

    private static User CreateUser(
        string email,
        string role,
        bool active)
    {
        return new User
        {
            Email = email,
            Names = email,
            Phone = "123",
            PasswordHash = "hash",
            Role = role,
            IsActive = active,
            DeactivatedAt = active ? null : DateTime.UtcNow
        };
    }

    private static Post CreatePost(
        string slug,
        PublicationStatus status)
    {
        return new Post
        {
            Slug = slug,
            Type = PostType.News,
            Status = status,
            TitleBg = slug,
            TitleEn = slug,
            BodyBg = "Текст",
            BodyEn = "Text",
            PublishedAt = status == PublicationStatus.Published
                ? DateTime.UtcNow
                : null
        };
    }

    private static Event CreateEvent(
        string slug,
        DateTime startAt,
        PublicationStatus status)
    {
        return new Event
        {
            Slug = slug,
            TitleBg = slug,
            TitleEn = slug,
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = startAt,
            EventType = EventType.Other,
            Status = status
        };
    }

    private static Dance CreateDance(
        string slug,
        bool active,
        string region)
    {
        return new Dance
        {
            Slug = slug,
            TitleBg = slug,
            TitleEn = slug,
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            Region = region,
            Active = active
        };
    }
}
