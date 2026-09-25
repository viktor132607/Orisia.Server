using Orisia.Server.Common.Responses.Calendar;

namespace Orisia.Server.Domain.Interfaces;

public interface ICalendarService
{
    Task<CalendarResponse> GetRangeAsync(DateTimeOffset from, DateTimeOffset to);
    Task<CalendarResponse> GetMonthAsync(int year, int month);
    Task<CalendarResponse> GetWeekAsync(DateTimeOffset date);
    Task<IReadOnlyCollection<CalendarOccurrenceResponse>> GetUpcomingAsync(int take = 5);
}
