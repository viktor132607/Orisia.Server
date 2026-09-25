using Orisia.Server.Core.Enums;

namespace Orisia.Server.Data.Entities;

public class Post : GenericEntity
{
    public required string Slug { get; set; }
    public PostType Type { get; set; }
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public required string BodyBg { get; set; }
    public required string BodyEn { get; set; }

    public string? ExcerptBg { get; set; }
    public string? ExcerptEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public bool Featured { get; set; }
    public DateTime? PublishedAt { get; set; }

    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
}
