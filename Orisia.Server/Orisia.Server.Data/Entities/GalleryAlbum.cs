namespace Orisia.Server.Data.Entities;

public class GalleryAlbum : GenericEntity
{
    public required string Slug { get; set; }
    public required string TitleBg { get; set; }
    public required string TitleEn { get; set; }
    public string? DescriptionBg { get; set; }
    public string? DescriptionEn { get; set; }

    public Guid? CoverMediaId { get; set; }
    public Media? CoverMedia { get; set; }

    public bool Active { get; set; } = true;
    public bool Featured { get; set; }
    public int SortOrder { get; set; }

    public ICollection<GalleryMedia> Items { get; set; } = new List<GalleryMedia>();
}
