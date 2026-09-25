using Orisia.Server.Domain.Media;

namespace Orisia.Server.Domain.Interfaces;

public interface IImageProcessor
{
    Task<ImageProcessingResult> ProcessAsync(
        Stream content,
        CancellationToken cancellationToken = default);
}
