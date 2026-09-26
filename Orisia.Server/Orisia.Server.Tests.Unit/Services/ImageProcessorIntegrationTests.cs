using Microsoft.Extensions.Options;
using Orisia.Server.API.Services;
using Orisia.Server.Common.Options;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public sealed class ImageProcessorIntegrationTests
{
    [Fact]
    public async Task NativeDecoderProcessesPngAndCreatesWebpThumbnail()
    {
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=");
        var processor = new SkiaSharpImageProcessor(Options.Create(new MediaStorageOptions()));
        await using var stream = new MemoryStream(bytes);
        var result = await processor.ProcessAsync(stream);
        Assert.Equal("image/png", result.DetectedMimeType);
        Assert.Equal(1, result.Width);
        Assert.Equal(1, result.Height);
        Assert.NotEmpty(result.ThumbnailBytes);
    }
}
