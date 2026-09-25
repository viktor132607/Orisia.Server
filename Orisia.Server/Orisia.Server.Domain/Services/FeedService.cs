using Orisia.Server.Common.Responses.Feed;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class FeedService(
    IPostRepository postRepository,
    IEventRepository eventRepository) : IFeedService
{
    public async Task<FeedResponse> GetAsync(
        string? type = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        bool? featured = null,
        int take = 20)
    {
        ValidateTake(take);

        string? normalizedType = NormalizeType(type);
        DateTime? fromUtc = from?.UtcDateTime;
        DateTime? toUtc = to?.UtcDateTime;

        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
        {
            throw new AppException("From date cannot be after to date.").SetStatusCode(400);
        }

        List<FeedItemResponse> items = [];

        bool includeEvents = normalizedType is null
            || normalizedType == FeedTypes.Event;

        bool includePosts = normalizedType != FeedTypes.Event;

        if (includePosts)
        {
            PostType? postType = FeedTypes.ToPostType(normalizedType);

            IEnumerable<Post> posts = await postRepository.GetPublishedForFeedAsync(
                fromUtc,
                toUtc,
                postType,
                featured,
                take);

            items.AddRange(posts.Select(MapPost));
        }

        if (includeEvents)
        {
            IEnumerable<Event> events = await eventRepository.GetPublishedAsync(
                fromUtc,
                toUtc,
                featured: featured);

            items.AddRange(events.Select(MapEvent));
        }

        FeedItemResponse[] result = items
            .OrderByDescending(item => item.Date)
            .ThenByDescending(item => item.Featured)
            .ThenBy(item => item.TitleBg)
            .Take(take)
            .ToArray();

        return new FeedResponse
        {
            Count = result.Length,
            Items = result
        };
    }

    public Task<FeedResponse> GetLatestAsync(int take = 20)
    {
        return GetAsync(take: take);
    }

    public Task<FeedResponse> GetFeaturedAsync(int take = 8)
    {
        return GetAsync(featured: true, take: take);
    }

    private static FeedItemResponse MapPost(Post post)
    {
        return new FeedItemResponse
        {
            Id = post.Id,
            Source = "post",
            Type = FeedTypes.FromPostType(post.Type),
            Slug = post.Slug,
            TitleBg = post.TitleBg,
            TitleEn = post.TitleEn,
            BodyBg = post.BodyBg,
            BodyEn = post.BodyEn,
            ExcerptBg = post.ExcerptBg,
            ExcerptEn = post.ExcerptEn,
            CoverMediaId = post.CoverMediaId,
            Featured = post.Featured,
            Date = post.PublishedAt!.Value,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.Names
        };
    }

    private static FeedItemResponse MapEvent(Event item)
    {
        return new FeedItemResponse
        {
            Id = item.Id,
            Source = "event",
            Type = FeedTypes.Event,
            Slug = item.Slug,
            TitleBg = item.TitleBg,
            TitleEn = item.TitleEn,
            BodyBg = item.DescriptionBg,
            BodyEn = item.DescriptionEn,
            CoverMediaId = item.CoverMediaId,
            Featured = item.Featured,
            Date = item.StartAt,
            EndAt = item.EndAt,
            EventType = item.EventType.ToString(),
            Location = item.Location
        };
    }

    private static string? NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        string normalized = type.Trim().ToLowerInvariant();
        if (!FeedTypes.IsValid(normalized))
        {
            throw new AppException(
                $"Unknown feed type '{type}'. Allowed values: {string.Join(", ", FeedTypes.All)}.")
                .SetStatusCode(400);
        }

        return normalized;
    }

    private static void ValidateTake(int take)
    {
        if (take is < 1 or > 100)
        {
            throw new AppException("Take must be between 1 and 100.").SetStatusCode(400);
        }
    }
}
