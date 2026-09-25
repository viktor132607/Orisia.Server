using Orisia.Server.Common.Requests.Users;
using Orisia.Server.Common.Responses.Users;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class UserService(IUserRepository userRepository, IAuthService authService) : IUserService
{
    public async Task<IEnumerable<UserResponse>?> GetAsync()
    {
        IEnumerable<User> users = await userRepository.GetAllAsync();
        return users.Select(MapUser);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        User? user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        return MapUser(user);
    }

    public async Task<UserResponse?> GetCurrentUserAsync()
    {
        Guid currentUserId = await GetCurrentUserIdAsync();
        return await GetByIdAsync(currentUserId);
    }

    public async Task<UserResponse?> UpdateCurrentUserAsync(UpdateCurrentUserRequest request)
    {
        Guid currentUserId = await GetCurrentUserIdAsync();

        return await UpdateAsync(new UpdateUserRequest
        {
            Id = currentUserId,
            Email = request.Email,
            Names = request.Names,
            Phone = request.Phone
        });
    }

    public async Task<UserResponse?> UpdateAsync(UpdateUserRequest request)
    {
        User? userBeforeUpdate = await userRepository.GetByIdAsync(request.Id);
        if (userBeforeUpdate is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        User updatedUserPayload = new()
        {
            Id = request.Id,
            Email = request.Email,
            Names = request.Names,
            Phone = request.Phone,
            PasswordHash = userBeforeUpdate.PasswordHash,
            Role = userBeforeUpdate.Role,
            RefreshToken = userBeforeUpdate.RefreshToken,
            RefreshTokenExpiryTime = userBeforeUpdate.RefreshTokenExpiryTime
        };

        User? updatedUser = await userRepository.UpdateAsync(updatedUserPayload);
        if (updatedUser is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        return MapUser(updatedUser);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Guid currentUserId = await GetCurrentUserIdAsync();
        if (currentUserId == id)
        {
            throw new AppException("You cannot delete your own account from the admin users endpoint.")
                .SetStatusCode(409);
        }

        _ = await GetByIdAsync(id);
        return await userRepository.DeleteAsync(id);
    }

    public async Task<UserResponse> SetRoleAsync(RoleChangeRequest request)
    {
        string role = request.Role.Trim();

        if (!Roles.IsValid(role))
        {
            throw new AppException($"Invalid role. Allowed roles: {Roles.Admin}, {Roles.Editor}, {Roles.User}.")
                .SetStatusCode(400);
        }

        Guid currentUserId = await GetCurrentUserIdAsync();
        if (currentUserId == request.UserId && role != Roles.Admin)
        {
            throw new AppException("An administrator cannot remove their own Admin role.")
                .SetStatusCode(409);
        }

        User? user = await userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        User updatedUserPayload = new()
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            PasswordHash = user.PasswordHash,
            Role = role,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
        };

        User? updatedUser = await userRepository.UpdateAsync(updatedUserPayload);
        if (updatedUser is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        return MapUser(updatedUser);
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        string? currentUserId = await authService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out Guid userId))
        {
            throw new AppException("Unauthorized").SetStatusCode(401);
        }

        return userId;
    }

    private static UserResponse MapUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            Role = user.Role ?? Roles.User
        };
    }
}
