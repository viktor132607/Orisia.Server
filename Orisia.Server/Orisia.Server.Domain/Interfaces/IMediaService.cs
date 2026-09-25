using Orisia.Server.Common.Responses.Media;
using Orisia.Server.Domain.Media;

namespace Orisia.Server.Domain.Interfaces;

public interface IMediaService
{
    Task<MediaResponse> UploadAsync(
        MediaUploadInput input,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<MediaResponse>> GetAllAsync();
    Task<MediaResponse> GetByIdAsync(Guid id);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
