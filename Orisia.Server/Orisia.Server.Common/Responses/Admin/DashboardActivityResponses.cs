using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Admin;

public class DashboardPostActivity
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string TitleBg { get; set; }
    public PostType Type { get; set; }
    public PublicationStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DashboardEventActivity
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string TitleBg { get; set; }
    public EventType EventType { get; set; }
    public DateTime StartAt { get; set; }
    public string? Location { get; set; }
}

public class DashboardInquiryActivity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public InquiryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardReviewActivity
{
    public Guid Id { get; set; }
    public required string AuthorName { get; set; }
    public int Rating { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
