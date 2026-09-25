using Moq;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class FeedServiceTests
{
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly FeedService _service;

    public FeedServiceTests()
    {
        _service = new FeedService(_posts.Object, _events.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldMergePostsAndEventsAndSortByDateDescending()
    {
        DateTime now = DateTime.UtcNow;

        _posts
            .Setup(x => x.GetPublishedForFeedAsync(
                null,
                null,
                null,
                null,
                20))
            .ReturnsAsync(
            [
                CreatePost("news", now.AddHours(-1))
            ]);

        _events
            .Setup(x => x.GetPublishedAsync(
                null,
                null,
                null,
                null))
            .ReturnsAsync(
            [
                CreateEvent("event", now.AddHours(2))
            ]);

        var result = await _service.GetAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("event", result.Items.First().Type);
        Assert.Equal("news", result.Items.Last().Type);
    }

    [Fact]
    public async Task GetAsync_WithBlogType_ShouldNotQueryEvents()
    {
        _posts
            .Setup(x => x.GetPublishedForFeedAsync(
                null,
                null,
                PostType.Blog,
                null,
                20))
            .ReturnsAsync(
            [
                CreatePost("blog", DateTime.UtcNow, PostType.Blog)
            ]);

        var result = await _service.GetAsync("blog");

        Assert.Single(result.Items);
        Assert.Equal("blog", result.Items.Single().Type);

        _events.Verify(
            x => x.GetPublishedAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<EventType?>(),
                It.IsAny<bool?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAsync_WithEventType_ShouldNotQueryPosts()
    {
        _events
            .Setup(x => x.GetPublishedAsync(
                null,
                null,
                null,
                true))
            .ReturnsAsync(
            [
                CreateEvent("featured-event", DateTime.UtcNow, true)
            ]);

        var result = await _service.GetAsync("event", featured: true);

        Assert.Single(result.Items);
        Assert.Equal("event", result.Items.Single().Source);

        _posts.Verify(
            x => x.GetPublishedForFeedAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<PostType?>(),
                It.IsAny<bool?>(),
                It.IsAny<int?>()),
            Times.Never);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("product")]
    public async Task GetAsync_ShouldRejectUnknownType(string type)
    {
        await Assert.ThrowsAsync<AppException>(() => _service.GetAsync(type));
    }

    [Fact]
    public async Task GetAsync_ShouldRejectInvalidDateRange()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<AppException>(() =>
            _service.GetAsync(from: now.AddDays(1), to: now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetAsync_ShouldRejectInvalidTake(int take)
    {
        await Assert.ThrowsAsync<AppException>(() => _service.GetAsync(take: take));
    }

    private static Post CreatePost(
        string slug,
        DateTime publishedAt,
        PostType type = PostType.News)
    {
        return new Post
        {
            Slug = slug,
            Type = type,
            Status = PublicationStatus.Published,
            TitleBg = "Публикация",
            TitleEn = "Post",
            BodyBg = "Текст",
            BodyEn = "Text",
            PublishedAt = publishedAt
        };
    }

    private static Event CreateEvent(
        string slug,
        DateTime startAt,
        bool featured = false)
    {
        return new Event
        {
            Slug = slug,
            TitleBg = "Събитие",
            TitleEn = "Event",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            StartAt = startAt,
            EventType = EventType.Performance,
            Featured = featured,
            Status = PublicationStatus.Published
        };
    }
}
