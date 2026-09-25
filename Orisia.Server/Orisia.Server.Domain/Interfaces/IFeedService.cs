using Orisia.Server.Common.Responses.Feed;

namespace Orisia.Server.Domain.Interfaces;

public interface IFeedService
{
    Task<FeedResponse> GetAsync(
        string? type = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        bool? featured = null,
        int take = 20);

    Task<FeedResponse> GetLatestAsync(int take = 20);
    Task<FeedResponse> GetFeaturedAsync(int take = 8);
}
