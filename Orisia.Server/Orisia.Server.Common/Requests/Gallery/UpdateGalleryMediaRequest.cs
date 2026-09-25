using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Gallery;

public class UpdateGalleryMediaRequest
{
    [MaxLength(500)]
    public string? CaptionBg { get; set; }

    [MaxLength(500)]
    public string? CaptionEn { get; set; }

    public int SortOrder { get; set; }
    public bool Active { get; set; }
}
