using System.ComponentModel.DataAnnotations;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Requests.Posts;

public class CreatePostRequest
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

    public Guid? CoverMediaId { get; set; }

    public bool Featured { get; set; }
}
