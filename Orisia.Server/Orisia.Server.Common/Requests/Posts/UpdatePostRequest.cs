using System.ComponentModel.DataAnnotations;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Requests.Posts;

public class UpdatePostRequest
{
    [MaxLength(180)]
    public string? Slug { get; set; }

    [Required]
    public PostType Type { get; set; }

    [Required, MaxLength(250)]
    public required string TitleBg { get; set; }

    [Required, MaxLength(250)]
    public required string TitleEn { get; set; }

    [Required]
    public required string BodyBg { get; set; }

    [Required]
    public required string BodyEn { get; set; }

    [MaxLength(500)]
    public string? ExcerptBg { get; set; }

    [MaxLength(500)]
    public string? ExcerptEn { get; set; }

    [MaxLength(70)]
    public string? SeoTitleBg { get; set; }

    [MaxLength(70)]
    public string? SeoTitleEn { get; set; }

    [MaxLength(180)]
    public string? SeoDescriptionBg { get; set; }

    [MaxLength(180)]
    public string? SeoDescriptionEn { get; set; }

    public Guid? CoverMediaId { get; set; }

    public bool Featured { get; set; }
}
