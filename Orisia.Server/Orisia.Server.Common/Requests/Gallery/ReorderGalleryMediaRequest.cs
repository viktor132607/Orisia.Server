using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Gallery;

public class ReorderGalleryMediaRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyCollection<GalleryMediaOrderItem> Items { get; set; }
}

public class GalleryMediaOrderItem
{
    public Guid GalleryMediaId { get; set; }
    public int SortOrder { get; set; }
}
