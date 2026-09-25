using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Media;
using SkiaSharp;

namespace Orisia.Server.API.Services;

public class SkiaSharpImageProcessor(
    IOptions<MediaStorageOptions> options) : IImageProcessor
{
    private readonly MediaStorageOptions _options = options.Value;

    public async Task<ImageProcessingResult> ProcessAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        await using MemoryStream buffer = new();
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await content.CopyToAsync(buffer, cancellationToken);
        byte[] bytes = buffer.ToArray();

        using SKData data = SKData.CreateCopy(bytes);
        using SKCodec codec = SKCodec.Create(data)
            ?? throw new InvalidDataException("Unable to decode image.");

        string mimeType = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Webp => "image/webp",
            SKEncodedImageFormat.Gif => "image/gif",
            _ => throw new InvalidDataException("Unsupported image format.")
        };

        using SKBitmap original = SKBitmap.Decode(bytes)
            ?? throw new InvalidDataException("Unable to decode image pixels.");

        int originalWidth = original.Width;
        int originalHeight = original.Height;

        double scale = Math.Min(
            1d,
            Math.Min(
                (double)_options.ThumbnailMaxWidth / originalWidth,
                (double)_options.ThumbnailMaxHeight / originalHeight));

        int targetWidth = Math.Max(1, (int)Math.Round(originalWidth * scale));
        int targetHeight = Math.Max(1, (int)Math.Round(originalHeight * scale));

        using SKBitmap resized = new(
            new SKImageInfo(
                targetWidth,
                targetHeight,
                SKColorType.Rgba8888,
                SKAlphaType.Premul));

        using (SKCanvas canvas = new(resized))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(
                original,
                new SKRect(0, 0, targetWidth, targetHeight),
                new SKPaint { IsAntialias = true });
        }

        using SKImage thumbnailImage = SKImage.FromBitmap(resized);
        using SKData encoded = thumbnailImage.Encode(SKEncodedImageFormat.Webp, 82)
            ?? throw new InvalidDataException("Unable to encode thumbnail.");

        return new ImageProcessingResult(
            mimeType,
            originalWidth,
            originalHeight,
            encoded.ToArray());
    }
}
