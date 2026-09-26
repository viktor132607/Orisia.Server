using Orisia.Server.Common.Requests.Groups;
using Orisia.Server.Common.Responses.Groups;

namespace Orisia.Server.Domain.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<GroupResponse>> GetPublicAsync();
    Task<GroupResponse> GetPublicBySlugAsync(string slug);
    Task<IEnumerable<GroupResponse>> GetAdminAsync();
    Task<GroupResponse> GetAdminByIdAsync(Guid id);
    Task<GroupResponse> CreateAsync(CreateGroupRequest request);
    Task<GroupResponse> UpdateAsync(Guid id, UpdateGroupRequest request);
    Task<bool> DeleteAsync(Guid id);
}
