namespace Orisia.Server.Common.Responses.Calendar;

public class CalendarResponse
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public required IReadOnlyCollection<CalendarOccurrenceResponse> Items { get; set; }
}
