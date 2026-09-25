using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
}
