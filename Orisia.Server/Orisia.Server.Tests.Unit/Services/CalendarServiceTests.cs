using Moq;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class CalendarServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IGroupRepository> _groups = new();
    private readonly CalendarService _service;

    public CalendarServiceTests()
    {
        _groups
            .Setup(x => x.GetActiveForCalendarAsync())
            .ReturnsAsync(Array.Empty<DanceGroup>());

        _service = new CalendarService(_events.Object, _groups.Object);
    }

    [Fact]
    public async Task GetRangeAsync_ShouldExpandRecurringEvents()
    {
        DateTime start = new(2026, 9, 1, 16, 0, 0, DateTimeKind.Utc);

        _events
            .Setup(x => x.GetCalendarCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(
            [
                new Event
                {
                    Id = Guid.NewGuid(),
                    Slug = "rehearsal",
                    TitleBg = "Репетиция",
                    TitleEn = "Rehearsal",
                    DescriptionBg = "Описание",
                    DescriptionEn = "Description",
                    StartAt = start,
                    EventType = EventType.Rehearsal,
                    Status = PublicationStatus.Published,
                    RecurrenceRule = "FREQ=WEEKLY;BYDAY=TU,TH"
                }
            ]);

        var result = await _service.GetRangeAsync(
            new DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 13, 23, 59, 59, TimeSpan.Zero));

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item =>
        {
            Assert.True(item.Recurring);
            Assert.Equal("event", item.Source);
        });
    }

    [Fact]
    public async Task GetRangeAsync_ShouldExpandWeeklyGroupScheduleInSofiaTime()
    {
        _events
            .Setup(x => x.GetCalendarCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<Event>());

        Guid groupId = Guid.NewGuid();
        _groups
            .Setup(x => x.GetActiveForCalendarAsync())
            .ReturnsAsync(
            [
                new DanceGroup
                {
                    Id = groupId,
                    Slug = "makamlii",
                    NameBg = "Макамлии",
                    NameEn = "Makamlii",
                    DescriptionBg = "Група",
                    DescriptionEn = "Group",
                    Active = true,
                    Schedules =
                    [
                        new DanceGroupSchedule
                        {
                            DayOfWeek = DayOfWeek.Thursday,
                            StartTime = new TimeOnly(19, 0),
                            DurationMinutes = 90
                        },
                        new DanceGroupSchedule
                        {
                            DayOfWeek = DayOfWeek.Saturday,
                            StartTime = new TimeOnly(19, 0),
                            DurationMinutes = 90
                        }
                    ]
                }
            ]);

        var result = await _service.GetRangeAsync(
            new DateTimeOffset(2026, 9, 21, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 27, 23, 59, 59, TimeSpan.Zero));

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item =>
        {
            Assert.Equal("group", item.Source);
            Assert.Equal(groupId, item.GroupId);
            Assert.Null(item.EventId);
            Assert.Equal("Макамлии", item.TitleBg);
        });
    }

    [Fact]
    public async Task GetMonthAsync_ShouldReturnExactMonthRange()
    {
        _events
            .Setup(x => x.GetCalendarCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<Event>());

        var result = await _service.GetMonthAsync(2026, 9);

        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), result.From);
        Assert.Equal(9, result.To.Month);
        Assert.Equal(30, result.To.Day);
    }

    [Fact]
    public async Task GetRangeAsync_ShouldRejectOversizedRange()
    {
        DateTimeOffset from = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<AppException>(() =>
            _service.GetRangeAsync(from, from.AddDays(371)));
    }
}
