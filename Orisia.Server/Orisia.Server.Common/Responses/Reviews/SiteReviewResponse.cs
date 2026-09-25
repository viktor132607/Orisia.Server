using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Reviews;

public class SiteReviewResponse
{
    public Guid Id { get; set; }
    public required string AuthorName { get; set; }
    public Guid? UserId { get; set; }
    public required string Content { get; set; }
    public int Rating { get; set; }
    public ReviewStatus Status { get; set; }
    public bool Featured { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public Guid? ModeratedById { get; set; }
    public string? ModeratedByName { get; set; }
}
