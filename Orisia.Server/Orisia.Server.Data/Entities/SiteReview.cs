using Orisia.Server.Core.Enums;

namespace Orisia.Server.Data.Entities;

public class SiteReview : GenericEntity
{
    public required string AuthorName { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public required string Content { get; set; }
    public int Rating { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public bool Featured { get; set; }

    public DateTime? ModeratedAt { get; set; }
    public Guid? ModeratedById { get; set; }
    public User? ModeratedBy { get; set; }
}
