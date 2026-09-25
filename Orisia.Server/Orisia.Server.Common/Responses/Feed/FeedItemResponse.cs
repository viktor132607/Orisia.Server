namespace Orisia.Server.Common.Responses.Feed;

public class FeedItemResponse
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public required string Type { get; set; }
    public required string Slug { get; set; }

    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public required string BodyBg { get; set; }
    public required string BodyEn { get; set; }

    public string? ExcerptBg { get; set; }
    public string? ExcerptEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public bool Featured { get; set; }

    public DateTime Date { get; set; }
    public DateTime? EndAt { get; set; }

    public string? EventType { get; set; }
    public string? Location { get; set; }

    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
}
