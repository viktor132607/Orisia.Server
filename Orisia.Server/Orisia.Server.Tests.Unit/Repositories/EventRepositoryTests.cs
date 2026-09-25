using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class EventRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly EventRepository _repository;

    public EventRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("EventRepositoryTests-" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new EventRepository(_context);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldIgnoreDeletedEvents()
    {
        _context.Events.AddRange(
            CreateEvent("active", DateTime.UtcNow.AddDays(1)),
            CreateEvent("deleted", DateTime.UtcNow.AddDays(2), isDeleted: true));

        await _context.SaveChangesAsync();

        Assert.NotNull(await _repository.GetBySlugAsync("active"));
        Assert.Null(await _repository.GetBySlugAsync("deleted"));
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldSupportExcludingCurrentEvent()
    {
        Event item = CreateEvent("same-slug", DateTime.UtcNow.AddDays(1));
        _context.Events.Add(item);
        await _context.SaveChangesAsync();

        Assert.True(await _repository.SlugExistsAsync("same-slug"));
        Assert.False(await _repository.SlugExistsAsync("same-slug", item.Id));
    }

    [Fact]
    public async Task GetPublishedAsync_ShouldFilterRangeTypeFeaturedAndStatus()
    {
        DateTime from = DateTime.UtcNow.Date;
        DateTime to = from.AddDays(7);

        _context.Events.AddRange(
            CreateEvent("performance", from.AddDays(1), PublicationStatus.Published, EventType.Performance, true),
            CreateEvent("rehearsal", from.AddDays(2), PublicationStatus.Published, EventType.Rehearsal, true),
            CreateEvent("draft", from.AddDays(3), PublicationStatus.Draft, EventType.Performance, true),
            CreateEvent("later", from.AddDays(10), PublicationStatus.Published, EventType.Performance, true));

        await _context.SaveChangesAsync();

        IEnumerable<Event> result = await _repository.GetPublishedAsync(
            from,
            to,
            EventType.Performance,
            true);

        Event only = Assert.Single(result);
        Assert.Equal("performance", only.Slug);
    }

    [Fact]
    public async Task GetPublishedAsync_ShouldIncludeEventOverlappingRange()
    {
        DateTime from = DateTime.UtcNow.Date.AddDays(3);
        Event item = CreateEvent(
            "multi-day",
            from.AddDays(-2),
            PublicationStatus.Published,
            EventType.Festival);

        item.EndAt = from.AddDays(1);

        _context.Events.Add(item);
        await _context.SaveChangesAsync();

        IEnumerable<Event> result = await _repository.GetPublishedAsync(from, from.AddDays(2));

        Assert.Single(result);
    }

    private static Event CreateEvent(
        string slug,
        DateTime startAt,
        PublicationStatus status = PublicationStatus.Published,
        EventType type = EventType.Other,
        bool featured = false,
        bool isDeleted = false)
    {
        return new Event
        {
            Slug = slug,
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = startAt,
            EventType = type,
            Featured = featured,
            Status = status,
            IsDeleted = isDeleted
        };
    }
}
