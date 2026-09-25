namespace Orisia.Server.Common.Options;

public class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    public string RootPath { get; set; } = "wwwroot/uploads/media";
    public string PublicBasePath { get; set; } = "/uploads/media";
    public long MaxFileSizeBytes { get; set; } = 15 * 1024 * 1024;
    public int ThumbnailMaxWidth { get; set; } = 640;
    public int ThumbnailMaxHeight { get; set; } = 640;
}
