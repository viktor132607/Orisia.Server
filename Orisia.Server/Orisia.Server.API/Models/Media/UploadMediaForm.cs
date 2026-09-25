using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.API.Models.Media;

public class UploadMediaForm
{
    [Required]
    public required IFormFile File { get; set; }

    [MaxLength(300)]
    public string? AltBg { get; set; }

    [MaxLength(300)]
    public string? AltEn { get; set; }
}
