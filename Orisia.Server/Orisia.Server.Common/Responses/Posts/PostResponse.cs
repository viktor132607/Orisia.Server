using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Responses.Posts;

public class PostResponse
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public PostType Type { get; set; }
    public PublicationStatus Status { get; set; }

    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public required string BodyBg { get; set; }
    public required string BodyEn { get; set; }

    public string? ExcerptBg { get; set; }
    public string? ExcerptEn { get; set; }

    public required string SeoTitleBg { get; set; }
    public required string SeoTitleEn { get; set; }
    public required string SeoDescriptionBg { get; set; }
    public required string SeoDescriptionEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public bool Featured { get; set; }
    public DateTime? PublishedAt { get; set; }

    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}
