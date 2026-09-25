using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Calendar;
using Xunit;

namespace Orisia.Server.Tests.Unit.Calendar;

public class RecurrenceExpanderTests
{
    [Fact]
    public void Expand_ShouldGenerateWeeklyOccurrencesInsideRange()
    {
        Event item = CreateEvent(
            new DateTime(2026, 9, 1, 16, 0, 0, DateTimeKind.Utc),
            "FREQ=WEEKLY;BYDAY=TU,TH");

        IReadOnlyList<EventOccurrence> result = RecurrenceExpander.Expand(
            item,
            new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 13, 23, 59, 59, DateTimeKind.Utc));

        Assert.Equal(2, result.Count);
        Assert.Equal(DayOfWeek.Tuesday, result[0].StartAt.DayOfWeek);
        Assert.Equal(DayOfWeek.Thursday, result[1].StartAt.DayOfWeek);
    }

    [Fact]
    public void Expand_ShouldRespectCountBeforeRequestedRange()
    {
        Event item = CreateEvent(
            new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            "FREQ=DAILY;COUNT=3");

        IReadOnlyList<EventOccurrence> result = RecurrenceExpander.Expand(
            item,
            new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc));

        Assert.Empty(result);
    }

    [Fact]
    public void Expand_ShouldPreserveEventDuration()
    {
        Event item = CreateEvent(
            new DateTime(2026, 9, 1, 16, 0, 0, DateTimeKind.Utc),
            "FREQ=DAILY;COUNT=2");

        item.EndAt = item.StartAt.AddHours(2);

        IReadOnlyList<EventOccurrence> result = RecurrenceExpander.Expand(
            item,
            item.StartAt,
            item.StartAt.AddDays(2));

        Assert.All(result, occurrence =>
            Assert.Equal(TimeSpan.FromHours(2), occurrence.EndAt - occurrence.StartAt));
    }

    [Fact]
    public void Expand_ShouldSupportLastDayOfMonth()
    {
        Event item = CreateEvent(
            new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            "FREQ=MONTHLY;BYMONTHDAY=-1;COUNT=3");

        IReadOnlyList<EventOccurrence> result = RecurrenceExpander.Expand(
            item,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc));

        Assert.Equal([31, 28, 31], result.Select(x => x.StartAt.Day).ToArray());
    }

    private static Event CreateEvent(DateTime startAt, string recurrenceRule)
    {
        return new Event
        {
            Slug = "event",
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = startAt,
            EventType = EventType.Rehearsal,
            Status = PublicationStatus.Published,
            RecurrenceRule = recurrenceRule
        };
    }
}
