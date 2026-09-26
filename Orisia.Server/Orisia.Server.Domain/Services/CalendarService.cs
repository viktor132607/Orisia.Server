using Orisia.Server.Common.Responses.Calendar;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Calendar;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class CalendarService(
    IEventRepository eventRepository,
    IGroupRepository groupRepository) : ICalendarService
{
    private static readonly TimeZoneInfo SofiaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia");

    public async Task<CalendarResponse> GetRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        DateTime fromUtc = from.UtcDateTime;
        DateTime toUtc = to.UtcDateTime;

        if (fromUtc > toUtc)
            throw new AppException("From date cannot be after to date.").SetStatusCode(400);

        if ((toUtc - fromUtc).TotalDays > 370)
            throw new AppException("Calendar range cannot exceed 370 days.").SetStatusCode(400);

        IReadOnlyCollection<CalendarOccurrenceResponse> items =
            await ExpandRangeAsync(fromUtc, toUtc);

        return new CalendarResponse
        {
            From = fromUtc,
            To = toUtc,
            Items = items
        };
    }

    public Task<CalendarResponse> GetMonthAsync(int year, int month)
    {
        if (year is < 2000 or > 2100 || month is < 1 or > 12)
            throw new AppException("Invalid calendar month.").SetStatusCode(400);

        DateTimeOffset from = new(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset to = from.AddMonths(1).AddTicks(-1);
        return GetRangeAsync(from, to);
    }

    public Task<CalendarResponse> GetWeekAsync(DateTimeOffset date)
    {
        DateTimeOffset utc = date.ToUniversalTime();
        int offset = ((int)utc.DayOfWeek + 6) % 7;

        DateTimeOffset from = new DateTimeOffset(
            utc.Date.AddDays(-offset),
            TimeSpan.Zero);

        DateTimeOffset to = from.AddDays(7).AddTicks(-1);
        return GetRangeAsync(from, to);
    }

    public async Task<IReadOnlyCollection<CalendarOccurrenceResponse>> GetUpcomingAsync(
        int take = 5)
    {
        if (take is < 1 or > 50)
            throw new AppException("Take must be between 1 and 50.").SetStatusCode(400);

        DateTime now = DateTime.UtcNow;
        DateTime horizon = now.AddYears(1);

        IReadOnlyCollection<CalendarOccurrenceResponse> items =
            await ExpandRangeAsync(now, horizon);

        return items
            .Where(item => (item.EndAt ?? item.StartAt) >= now)
            .OrderBy(item => item.StartAt)
            .Take(take)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<CalendarOccurrenceResponse>> ExpandRangeAsync(
        DateTime fromUtc,
        DateTime toUtc)
    {
        IEnumerable<Event> candidates =
            await eventRepository.GetCalendarCandidatesAsync(fromUtc, toUtc);

        IEnumerable<DanceGroup> groups =
            await groupRepository.GetActiveForCalendarAsync();

        List<CalendarOccurrenceResponse> occurrences = [];

        foreach (Event item in candidates)
        {
            foreach (EventOccurrence occurrence in RecurrenceExpander.Expand(item, fromUtc, toUtc))
                occurrences.Add(MapEvent(item, occurrence));
        }

        foreach (DanceGroup group in groups)
            occurrences.AddRange(ExpandGroup(group, fromUtc, toUtc));

        return occurrences
            .OrderBy(item => item.StartAt)
            .ThenBy(item => item.EventType)
            .ThenBy(item => item.TitleBg)
            .ToArray();
    }

    private static IEnumerable<CalendarOccurrenceResponse> ExpandGroup(
        DanceGroup group,
        DateTime fromUtc,
        DateTime toUtc)
    {
        DateOnly firstLocalDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(fromUtc, SofiaTimeZone));

        DateOnly lastLocalDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(toUtc, SofiaTimeZone));

        foreach (DanceGroupSchedule schedule in group.Schedules.Where(item => !item.IsDeleted))
        {
            for (DateOnly date = firstLocalDate; date <= lastLocalDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != schedule.DayOfWeek) continue;

                DateTime localStart = DateTime.SpecifyKind(
                    date.ToDateTime(schedule.StartTime),
                    DateTimeKind.Unspecified);

                DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, SofiaTimeZone);
                DateTime endUtc = startUtc.AddMinutes(schedule.DurationMinutes);

                if (endUtc < fromUtc || startUtc > toUtc) continue;

                yield return new CalendarOccurrenceResponse
                {
                    OccurrenceId = $"{group.Id:N}:group:{startUtc.Ticks}",
                    Source = "group",
                    EventId = null,
                    GroupId = group.Id,
                    Slug = group.Slug,
                    TitleBg = group.NameBg,
                    TitleEn = group.NameEn,
                    DescriptionBg = group.DescriptionBg,
                    DescriptionEn = group.DescriptionEn,
                    StartAt = startUtc,
                    EndAt = endUtc,
                    AllDay = false,
                    EventType = EventType.Rehearsal,
                    Location = group.Location,
                    CoverMediaId = null,
                    Featured = false,
                    Recurring = true
                };
            }
        }
    }

    private static CalendarOccurrenceResponse MapEvent(
        Event item,
        EventOccurrence occurrence)
    {
        return new CalendarOccurrenceResponse
        {
            OccurrenceId = $"{item.Id:N}:{occurrence.StartAt.Ticks}",
            Source = "event",
            EventId = item.Id,
            GroupId = null,
            Slug = item.Slug,
            TitleBg = item.TitleBg,
            TitleEn = item.TitleEn,
            DescriptionBg = item.DescriptionBg,
            DescriptionEn = item.DescriptionEn,
            StartAt = occurrence.StartAt,
            EndAt = occurrence.EndAt,
            AllDay = item.AllDay,
            EventType = item.EventType,
            Location = item.Location,
            CoverMediaId = item.CoverMediaId,
            Featured = item.Featured,
            Recurring = !string.IsNullOrWhiteSpace(item.RecurrenceRule)
        };
    }
}
