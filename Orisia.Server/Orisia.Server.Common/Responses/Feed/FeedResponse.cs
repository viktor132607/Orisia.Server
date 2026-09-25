namespace Orisia.Server.Common.Responses.Feed;

public class FeedResponse
{
    public int Count { get; set; }
    public required IReadOnlyCollection<FeedItemResponse> Items { get; set; }
}
