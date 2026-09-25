namespace Orisia.Server.Common.Responses.Admin;

public class UserDashboardMetrics
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Admins { get; set; }
    public int Editors { get; set; }
}

public class PostDashboardMetrics
{
    public int Total { get; set; }
    public int Published { get; set; }
    public int Draft { get; set; }
    public int Archived { get; set; }
    public int Featured { get; set; }
}

public class EventDashboardMetrics
{
    public int Total { get; set; }
    public int Published { get; set; }
    public int Draft { get; set; }
    public int Archived { get; set; }
    public int Upcoming { get; set; }
    public int Past { get; set; }
    public int Recurring { get; set; }
    public int Featured { get; set; }
}

public class GalleryDashboardMetrics
{
    public int Albums { get; set; }
    public int ActiveAlbums { get; set; }
    public int FeaturedAlbums { get; set; }
    public int Items { get; set; }
    public int ActiveItems { get; set; }
}

public class ReviewDashboardMetrics
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Featured { get; set; }
    public double? AverageApprovedRating { get; set; }
}

public class InquiryDashboardMetrics
{
    public int Total { get; set; }
    public int New { get; set; }
    public int Read { get; set; }
    public int Answered { get; set; }
    public int Archived { get; set; }
}

public class MediaDashboardMetrics
{
    public int Total { get; set; }
    public long TotalSizeBytes { get; set; }
}

public class DanceDashboardMetrics
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Regions { get; set; }
}
