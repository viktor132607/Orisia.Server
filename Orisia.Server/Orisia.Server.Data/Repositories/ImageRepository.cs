using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class ImageRepository(ApplicationDbContext context) : Repository<Image>(context), IImageRepository
{
    
}
