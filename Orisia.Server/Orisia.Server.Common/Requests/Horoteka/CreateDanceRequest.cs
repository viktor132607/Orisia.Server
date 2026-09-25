using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Horoteka;

public class CreateDanceRequest
{
    [MaxLength(180)]
    public string? Slug { get; set; }

    [Required, MaxLength(250)]
    public required string TitleBg { get; set; }

    [Required, MaxLength(250)]
    public required string TitleEn { get; set; }

    [Required]
    public required string DescriptionBg { get; set; }

    [Required]
    public required string DescriptionEn { get; set; }

    [MaxLength(120)]
    public string? Region { get; set; }

    [MaxLength(120)]
    public string? Rhythm { get; set; }

    [Url, MaxLength(1000)]
    public string? VideoUrl { get; set; }

    public Guid? ThumbnailMediaId { get; set; }

    [Range(1, 86400)]
    public int? DurationSeconds { get; set; }

    public int SortOrder { get; set; }
    public bool Active { get; set; } = true;
}
