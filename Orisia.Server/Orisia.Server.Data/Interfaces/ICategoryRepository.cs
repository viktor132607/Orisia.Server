using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> IsNameAlreadyUsed(string name);
}
