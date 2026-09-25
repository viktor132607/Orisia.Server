namespace Orisia.Server.Data.Entities;

public class GalleryMedia : GenericEntity
{
    public Guid GalleryAlbumId { get; set; }
    public GalleryAlbum? GalleryAlbum { get; set; }

    public Guid MediaId { get; set; }
    public Media? Media { get; set; }

    public string? CaptionBg { get; set; }
    public string? CaptionEn { get; set; }

    public int SortOrder { get; set; }
    public bool Active { get; set; } = true;
}
