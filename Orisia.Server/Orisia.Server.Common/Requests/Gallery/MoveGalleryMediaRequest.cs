namespace Orisia.Server.Common.Requests.Gallery;

public class MoveGalleryMediaRequest
{
    public Guid TargetAlbumId { get; set; }
    public int? SortOrder { get; set; }
}
