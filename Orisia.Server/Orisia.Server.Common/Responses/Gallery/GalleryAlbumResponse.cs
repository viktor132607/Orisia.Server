namespace Orisia.Server.Common.Responses.Gallery;

public class GalleryAlbumResponse
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public string? DescriptionBg { get; set; }
    public string? DescriptionEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public string? CoverUrl { get; set; }
    public string? CoverThumbnailUrl { get; set; }

    public bool Active { get; set; }
    public bool Featured { get; set; }
    public int SortOrder { get; set; }

    public required IReadOnlyCollection<GalleryMediaResponse> Items { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}
