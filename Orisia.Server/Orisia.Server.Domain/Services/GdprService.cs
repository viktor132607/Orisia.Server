using Microsoft.EntityFrameworkCore;
using Orisia.Server.Common.Responses.Gdpr;
using Orisia.Server.Common.Responses.Users;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class GdprService(
    ApplicationDbContext context,
    IAuthService authService,
    IUserRepository userRepository) : IGdprService
{
    public async Task<GdprExportResponse> ExportCurrentUserDataAsync()
    {
        Guid userId = await GetCurrentUserIdAsync();

        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId && !item.IsDeleted);

        if (user is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        return new GdprExportResponse
        {
            RequestedAtUtc = DateTime.UtcNow,
            User = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Names = user.Names,
                Phone = user.Phone,
                Role = user.Role
            }
        };
    }

    public async Task<GdprDeleteResponse> DeleteCurrentUserDataAsync()
    {
        Guid userId = await GetCurrentUserIdAsync();

        User? user = await context.Users
            .FirstOrDefaultAsync(item => item.Id == userId && !item.IsDeleted);

        if (user is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        user.Email = "deleted-" + user.Id + "@orisia.local";
        user.Names = "Deleted user";
        user.Phone = string.Empty;
        user.PasswordHash = string.Empty;
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.IsDeleted = true;
        user.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new GdprDeleteResponse
        {
            Deleted = true,
            Message = "Your personal account data has been anonymized and marked for deletion."
        };
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        string? currentUserId = await authService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out Guid userId))
        {
            throw new AppException("Unauthorized").SetStatusCode(401);
        }

        User? user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new AppException("User not found.").SetStatusCode(404);
        }

        return userId;
    }
}
