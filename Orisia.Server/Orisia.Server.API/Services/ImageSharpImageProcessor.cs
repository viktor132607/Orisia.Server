using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Orisia.Server.API.Services;

public class ImageSharpImageProcessor(
    IOptions<MediaStorageOptions> options) : IImageProcessor
{
    private readonly MediaStorageOptions _options = options.Value;

    public async Task<ImageProcessingResult> ProcessAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        using Image image = await Image.LoadAsync(content, cancellationToken);
        string mimeType = image.Metadata.DecodedImageFormat?.DefaultMimeType
            ?? throw new InvalidDataException("Unable to detect image format.");

        int width = image.Width;
        int height = image.Height;

        image.Mutate(context => context.AutoOrient());

        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(
                _options.ThumbnailMaxWidth,
                _options.ThumbnailMaxHeight)
        }));

        await using MemoryStream thumbnail = new();
        await image.SaveAsWebpAsync(thumbnail, cancellationToken);

        return new ImageProcessingResult(
            mimeType,
            width,
            height,
            thumbnail.ToArray());
    }
}
