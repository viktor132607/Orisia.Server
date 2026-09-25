using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailAlreadyUsed(string email);
    Task<bool> IsEmailAlreadyUsedByOtherUser(string email, Guid userId);
}
