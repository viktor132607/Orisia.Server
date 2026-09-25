using Orisia.Server.Common.Requests.Users;
using Orisia.Server.Common.Responses.Users;

namespace Orisia.Server.Domain.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>?> GetAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse?> GetCurrentUserAsync();
    Task<UserResponse?> UpdateCurrentUserAsync(UpdateCurrentUserRequest request);
    Task<UserResponse?> UpdateAsync(UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<UserResponse> SetRoleAsync(RoleChangeRequest request);
    Task<UserResponse> DeactivateAsync(Guid id);
    Task<UserResponse> ActivateAsync(Guid id);
}
