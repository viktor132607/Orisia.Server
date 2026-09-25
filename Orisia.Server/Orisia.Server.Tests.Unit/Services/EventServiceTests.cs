using Moq;
using Orisia.Server.Common.Requests.Events;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly EventService _service;

    public EventServiceTests()
    {
        _service = new EventService(_events.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldNormalizeDatesSlugAndRecurrenceRule()
    {
        _events.Setup(x => x.SlugExistsAsync("narodna-repetitsiya", null)).ReturnsAsync(false);
        _events.Setup(x => x.AddAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event item) => item);

        var result = await _service.CreateAsync(new CreateEventRequest
        {
            TitleBg = "Народна репетиция",
            TitleEn = "",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = new DateTimeOffset(2026, 10, 1, 19, 0, 0, TimeSpan.FromHours(3)),
            EndAt = new DateTimeOffset(2026, 10, 1, 21, 0, 0, TimeSpan.FromHours(3)),
            EventType = EventType.Rehearsal,
            RecurrenceRule = "rrule:FREQ=WEEKLY;BYDAY=TU,TH"
        });

        Assert.Equal("narodna-repetitsiya", result.Slug);
        Assert.Equal(DateTimeKind.Utc, result.StartAt.Kind);
        Assert.Equal("FREQ=WEEKLY;BYDAY=TU,TH", result.RecurrenceRule);
        Assert.Equal(PublicationStatus.Draft, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEndBeforeStart()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateEventRequest
        {
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = start,
            EndAt = start.AddMinutes(-1),
            EventType = EventType.Other
        }));
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidRecurrenceRule()
    {
        _events.Setup(x => x.SlugExistsAsync("event", null)).ReturnsAsync(false);

        await Assert.ThrowsAsync<AppException>(() => _service.CreateAsync(new CreateEventRequest
        {
            Slug = "event",
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = DateTimeOffset.UtcNow,
            EventType = EventType.Rehearsal,
            RecurrenceRule = "FREQ=SOMETIMES"
        }));
    }

    [Fact]
    public async Task GetPublishedAsync_ShouldRejectInvalidRange()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<AppException>(() =>
            _service.GetPublishedAsync(now.AddDays(1), now));
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_ShouldRejectDraft()
    {
        _events.Setup(x => x.GetBySlugAsync("draft"))
            .ReturnsAsync(CreateEvent("draft", PublicationStatus.Draft));

        await Assert.ThrowsAsync<AppException>(() =>
            _service.GetPublishedBySlugAsync("draft"));
    }

    [Fact]
    public async Task UnpublishAsync_ShouldReturnDraft()
    {
        Guid id = Guid.NewGuid();
        _events.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(CreateEvent("event", PublicationStatus.Published, id));
        _events.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event item) => item);

        var result = await _service.UnpublishAsync(id);

        Assert.Equal(PublicationStatus.Draft, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetUpcomingAsync_ShouldRejectInvalidTake(int take)
    {
        await Assert.ThrowsAsync<AppException>(() => _service.GetUpcomingAsync(take: take));
    }

    private static Event CreateEvent(
        string slug,
        PublicationStatus status,
        Guid? id = null)
    {
        return new Event
        {
            Id = id ?? Guid.NewGuid(),
            Slug = slug,
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = DateTime.UtcNow,
            EventType = EventType.Other,
            Status = status
        };
    }
}
