using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class CategoryRepository(ApplicationDbContext context) : Repository<Category>(context), ICategoryRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<bool> IsNameAlreadyUsed(string name)
    {
        return _context.Users.Any(u => u.Email == name && u.IsDeleted == false);   
    }
}
