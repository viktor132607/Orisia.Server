using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Events;

public class EventResponse
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }

    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public required string DescriptionBg { get; set; }
    public required string DescriptionEn { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool AllDay { get; set; }

    public EventType EventType { get; set; }
    public string? Location { get; set; }

    public Guid? CoverMediaId { get; set; }
    public bool Featured { get; set; }

    public PublicationStatus Status { get; set; }
    public string? RecurrenceRule { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}
