namespace Orisia.Server.Common.Responses.Admin;

public class AdminDashboardResponse
{
    public DateTime GeneratedAt { get; set; }

    public required UserDashboardMetrics Users { get; set; }
    public required PostDashboardMetrics Posts { get; set; }
    public required EventDashboardMetrics Events { get; set; }
    public required GalleryDashboardMetrics Gallery { get; set; }
    public required ReviewDashboardMetrics Reviews { get; set; }
    public required InquiryDashboardMetrics Inquiries { get; set; }
    public required MediaDashboardMetrics Media { get; set; }
    public required DanceDashboardMetrics Dances { get; set; }

    public required IReadOnlyCollection<DashboardPostActivity> RecentPosts { get; set; }
    public required IReadOnlyCollection<DashboardEventActivity> UpcomingEvents { get; set; }
    public required IReadOnlyCollection<DashboardInquiryActivity> RecentInquiries { get; set; }
    public required IReadOnlyCollection<DashboardReviewActivity> PendingReviews { get; set; }
}
