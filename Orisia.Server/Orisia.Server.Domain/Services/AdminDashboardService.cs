using Microsoft.EntityFrameworkCore;
using Orisia.Server.Common.Responses.Admin;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class AdminDashboardService(ApplicationDbContext context) : IAdminDashboardService
{
    public async Task<AdminDashboardResponse> GetAsync()
    {
        DateTime now = DateTime.UtcNow;

        UserDashboardMetrics users = new()
        {
            Total = await context.Users.CountAsync(user => !user.IsDeleted),
            Active = await context.Users.CountAsync(user => !user.IsDeleted && user.IsActive),
            Inactive = await context.Users.CountAsync(user => !user.IsDeleted && !user.IsActive),
            Admins = await context.Users.CountAsync(user =>
                !user.IsDeleted && user.Role == Roles.Admin),
            Editors = await context.Users.CountAsync(user =>
                !user.IsDeleted && user.Role == Roles.Editor)
        };

        PostDashboardMetrics posts = new()
        {
            Total = await context.Posts.CountAsync(post => !post.IsDeleted),
            Published = await context.Posts.CountAsync(post =>
                !post.IsDeleted && post.Status == PublicationStatus.Published),
            Draft = await context.Posts.CountAsync(post =>
                !post.IsDeleted && post.Status == PublicationStatus.Draft),
            Archived = await context.Posts.CountAsync(post =>
                !post.IsDeleted && post.Status == PublicationStatus.Archived),
            Featured = await context.Posts.CountAsync(post =>
                !post.IsDeleted && post.Featured)
        };

        EventDashboardMetrics events = new()
        {
            Total = await context.Events.CountAsync(item => !item.IsDeleted),
            Published = await context.Events.CountAsync(item =>
                !item.IsDeleted && item.Status == PublicationStatus.Published),
            Draft = await context.Events.CountAsync(item =>
                !item.IsDeleted && item.Status == PublicationStatus.Draft),
            Archived = await context.Events.CountAsync(item =>
                !item.IsDeleted && item.Status == PublicationStatus.Archived),
            Upcoming = await context.Events.CountAsync(item =>
                !item.IsDeleted
                && item.Status == PublicationStatus.Published
                && (item.EndAt ?? item.StartAt) >= now),
            Past = await context.Events.CountAsync(item =>
                !item.IsDeleted
                && item.Status == PublicationStatus.Published
                && (item.EndAt ?? item.StartAt) < now),
            Recurring = await context.Events.CountAsync(item =>
                !item.IsDeleted && item.RecurrenceRule != null),
            Featured = await context.Events.CountAsync(item =>
                !item.IsDeleted && item.Featured)
        };

        GalleryDashboardMetrics gallery = new()
        {
            Albums = await context.GalleryAlbums.CountAsync(album => !album.IsDeleted),
            ActiveAlbums = await context.GalleryAlbums.CountAsync(album =>
                !album.IsDeleted && album.Active),
            FeaturedAlbums = await context.GalleryAlbums.CountAsync(album =>
                !album.IsDeleted && album.Featured),
            Items = await context.GalleryMedia.CountAsync(item => !item.IsDeleted),
            ActiveItems = await context.GalleryMedia.CountAsync(item =>
                !item.IsDeleted && item.Active)
        };

        int approvedReviewCount = await context.SiteReviews.CountAsync(review =>
            !review.IsDeleted && review.Status == ReviewStatus.Approved);

        double? averageApprovedRating = approvedReviewCount == 0
            ? null
            : await context.SiteReviews
                .Where(review =>
                    !review.IsDeleted
                    && review.Status == ReviewStatus.Approved)
                .AverageAsync(review => (double)review.Rating);

        ReviewDashboardMetrics reviews = new()
        {
            Total = await context.SiteReviews.CountAsync(review => !review.IsDeleted),
            Pending = await context.SiteReviews.CountAsync(review =>
                !review.IsDeleted && review.Status == ReviewStatus.Pending),
            Approved = approvedReviewCount,
            Rejected = await context.SiteReviews.CountAsync(review =>
                !review.IsDeleted && review.Status == ReviewStatus.Rejected),
            Featured = await context.SiteReviews.CountAsync(review =>
                !review.IsDeleted
                && review.Status == ReviewStatus.Approved
                && review.Featured),
            AverageApprovedRating = averageApprovedRating
        };

        InquiryDashboardMetrics inquiries = new()
        {
            Total = await context.ContactInquiries.CountAsync(item => !item.IsDeleted),
            New = await context.ContactInquiries.CountAsync(item =>
                !item.IsDeleted && item.Status == InquiryStatus.New),
            Read = await context.ContactInquiries.CountAsync(item =>
                !item.IsDeleted && item.Status == InquiryStatus.Read),
            Answered = await context.ContactInquiries.CountAsync(item =>
                !item.IsDeleted && item.Status == InquiryStatus.Answered),
            Archived = await context.ContactInquiries.CountAsync(item =>
                !item.IsDeleted && item.Status == InquiryStatus.Archived)
        };

        MediaDashboardMetrics media = new()
        {
            Total = await context.Media.CountAsync(item => !item.IsDeleted),
            TotalSizeBytes = await context.Media
                .Where(item => !item.IsDeleted)
                .SumAsync(item => (long?)item.SizeBytes) ?? 0
        };

        DanceDashboardMetrics dances = new()
        {
            Total = await context.Dances.CountAsync(item => !item.IsDeleted),
            Active = await context.Dances.CountAsync(item =>
                !item.IsDeleted && item.Active),
            Inactive = await context.Dances.CountAsync(item =>
                !item.IsDeleted && !item.Active),
            Regions = await context.Dances
                .Where(item =>
                    !item.IsDeleted
                    && item.Region != null
                    && item.Region != string.Empty)
                .Select(item => item.Region)
                .Distinct()
                .CountAsync()
        };

        DashboardPostActivity[] recentPosts = await context.Posts
            .AsNoTracking()
            .Where(post => !post.IsDeleted)
            .OrderByDescending(post => post.ModifiedOn)
            .Take(5)
            .Select(post => new DashboardPostActivity
            {
                Id = post.Id,
                Slug = post.Slug,
                TitleBg = post.TitleBg,
                Type = post.Type,
                Status = post.Status,
                UpdatedAt = post.ModifiedOn
            })
            .ToArrayAsync();

        DashboardEventActivity[] upcomingEvents = await context.Events
            .AsNoTracking()
            .Where(item =>
                !item.IsDeleted
                && item.Status == PublicationStatus.Published
                && (item.EndAt ?? item.StartAt) >= now)
            .OrderBy(item => item.StartAt)
            .Take(5)
            .Select(item => new DashboardEventActivity
            {
                Id = item.Id,
                Slug = item.Slug,
                TitleBg = item.TitleBg,
                EventType = item.EventType,
                StartAt = item.StartAt,
                Location = item.Location
            })
            .ToArrayAsync();

        DashboardInquiryActivity[] recentInquiries = await context.ContactInquiries
            .AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedOn)
            .Take(5)
            .Select(item => new DashboardInquiryActivity
            {
                Id = item.Id,
                Name = item.Name,
                Subject = item.Subject,
                Status = item.Status,
                CreatedAt = item.CreatedOn
            })
            .ToArrayAsync();

        DashboardReviewActivity[] pendingReviews = await context.SiteReviews
            .AsNoTracking()
            .Where(review =>
                !review.IsDeleted
                && review.Status == ReviewStatus.Pending)
            .OrderByDescending(review => review.CreatedOn)
            .Take(5)
            .Select(review => new DashboardReviewActivity
            {
                Id = review.Id,
                AuthorName = review.AuthorName,
                Rating = review.Rating,
                Status = review.Status,
                CreatedAt = review.CreatedOn
            })
            .ToArrayAsync();

        return new AdminDashboardResponse
        {
            GeneratedAt = now,
            Users = users,
            Posts = posts,
            Events = events,
            Gallery = gallery,
            Reviews = reviews,
            Inquiries = inquiries,
            Media = media,
            Dances = dances,
            RecentPosts = recentPosts,
            UpcomingEvents = upcomingEvents,
            RecentInquiries = recentInquiries,
            PendingReviews = pendingReviews
        };
    }
}
