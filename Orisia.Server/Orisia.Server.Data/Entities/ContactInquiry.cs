using Orisia.Server.Core.Enums;

namespace Orisia.Server.Data.Entities;

public class ContactInquiry : GenericEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    public DateTime? ReadAt { get; set; }

    public string? AnswerText { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public Guid? AnsweredById { get; set; }
    public User? AnsweredBy { get; set; }

    public DateTime? ArchivedAt { get; set; }
}
