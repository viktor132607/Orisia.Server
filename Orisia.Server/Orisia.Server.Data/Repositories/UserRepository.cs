using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
{
    public Task<bool> IsEmailAlreadyUsed(string email)
    {
        string normalized = email.Trim().ToLowerInvariant();

        return Context.Users.AnyAsync(user =>
            user.Email == normalized
            && !user.IsDeleted);
    }

    public Task<bool> IsEmailAlreadyUsedByOtherUser(string email, Guid userId)
    {
        string normalized = email.Trim().ToLowerInvariant();

        return Context.Users.AnyAsync(user =>
            user.Id != userId
            && user.Email == normalized
            && !user.IsDeleted);
    }
}
