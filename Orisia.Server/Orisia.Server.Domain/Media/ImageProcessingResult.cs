namespace Orisia.Server.Domain.Media;

public sealed record ImageProcessingResult(
    string DetectedMimeType,
    int Width,
    int Height,
    byte[] ThumbnailBytes);
