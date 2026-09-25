namespace Orisia.Server.Data.Entities;

public class Media : GenericEntity
{
    public required string OriginalFileName { get; set; }
    public required string StorageKey { get; set; }
    public string? ThumbnailStorageKey { get; set; }

    public required string MimeType { get; set; }
    public required string Extension { get; set; }
    public long SizeBytes { get; set; }
    public required string Sha256 { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public string? AltBg { get; set; }
    public string? AltEn { get; set; }

    public Guid? UploadedById { get; set; }
    public User? UploadedBy { get; set; }
}
