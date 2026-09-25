using Orisia.Server.Common.Requests.Horoteka;
using Orisia.Server.Common.Responses.Horoteka;

namespace Orisia.Server.Domain.Interfaces;

public interface IDanceService
{
    Task<IEnumerable<DanceResponse>> GetPublicAsync(string? region = null);
    Task<DanceResponse> GetPublicBySlugAsync(string slug);

    Task<IEnumerable<DanceResponse>> GetAdminAsync();
    Task<DanceResponse> GetAdminByIdAsync(Guid id);

    Task<DanceResponse> CreateAsync(CreateDanceRequest request);
    Task<DanceResponse> UpdateAsync(Guid id, UpdateDanceRequest request);
    Task ReorderAsync(ReorderDancesRequest request);
    Task<bool> DeleteAsync(Guid id);
}
