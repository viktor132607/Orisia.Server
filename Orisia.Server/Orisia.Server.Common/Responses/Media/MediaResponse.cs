namespace Orisia.Server.Common.Responses.Media;

public class MediaResponse
{
    public Guid Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required string MimeType { get; set; }
    public required string Extension { get; set; }
    public long SizeBytes { get; set; }
    public required string Sha256 { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public string? AltBg { get; set; }
    public string? AltEn { get; set; }

    public required string Url { get; set; }
    public string? ThumbnailUrl { get; set; }

    public Guid? UploadedById { get; set; }
    public string? UploadedByName { get; set; }

    public DateTime CreatedOn { get; set; }
}
