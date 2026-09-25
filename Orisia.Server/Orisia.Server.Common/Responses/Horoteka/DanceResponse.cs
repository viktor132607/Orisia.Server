namespace Orisia.Server.Common.Responses.Horoteka;

public class DanceResponse
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }

    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public required string DescriptionBg { get; set; }
    public required string DescriptionEn { get; set; }

    public string? Region { get; set; }
    public string? Rhythm { get; set; }
    public string? VideoUrl { get; set; }

    public Guid? ThumbnailMediaId { get; set; }
    public string? ThumbnailUrl { get; set; }

    public int? DurationSeconds { get; set; }
    public int SortOrder { get; set; }
    public bool Active { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}
