namespace Orisia.Server.Domain.Media;

public sealed record MediaUploadInput(
    Stream Content,
    string OriginalFileName,
    string ContentType,
    long Length,
    string? AltBg,
    string? AltEn);
