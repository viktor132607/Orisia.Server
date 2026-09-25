using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Calendar;

public class CalendarOccurrenceResponse
{
    public required string OccurrenceId { get; set; }
    public Guid EventId { get; set; }
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
    public bool Recurring { get; set; }
}
