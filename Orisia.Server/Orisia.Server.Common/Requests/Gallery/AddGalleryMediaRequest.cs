using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Gallery;

public class AddGalleryMediaRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyCollection<GalleryMediaInput> Items { get; set; }
}

public class GalleryMediaInput
{
    [Required]
    public Guid MediaId { get; set; }

    [MaxLength(500)]
    public string? CaptionBg { get; set; }

    [MaxLength(500)]
    public string? CaptionEn { get; set; }

    public int? SortOrder { get; set; }
    public bool Active { get; set; } = true;
}
