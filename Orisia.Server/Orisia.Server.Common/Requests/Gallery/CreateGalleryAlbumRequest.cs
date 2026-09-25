using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Gallery;

public class CreateGalleryAlbumRequest
{
    [MaxLength(180)]
    public string? Slug { get; set; }

    [Required, MaxLength(250)]
    public required string TitleBg { get; set; }

    [Required, MaxLength(250)]
    public required string TitleEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionBg { get; set; }

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public bool Active { get; set; } = true;
    public bool Featured { get; set; }
    public int SortOrder { get; set; }
}
