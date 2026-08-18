using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviews(Guid productId);
}
