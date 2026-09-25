namespace Orisia.Server.Common.Responses.Gallery;

public class GalleryMediaResponse
{
    public Guid Id { get; set; }
    public Guid MediaId { get; set; }
    public required string Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? AltBg { get; set; }
    public string? AltEn { get; set; }
    public string? CaptionBg { get; set; }
    public string? CaptionEn { get; set; }
    public int SortOrder { get; set; }
    public bool Active { get; set; }
}
