using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Inquiries;

public class ContactInquiryResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }

    public InquiryStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public DateTime? ReadAt { get; set; }

    public string? AnswerText { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public Guid? AnsweredById { get; set; }
    public string? AnsweredByName { get; set; }

    public DateTime? ArchivedAt { get; set; }
}
