using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<bool> AddRange(ICollection<OrderItem> orderItems);
}
