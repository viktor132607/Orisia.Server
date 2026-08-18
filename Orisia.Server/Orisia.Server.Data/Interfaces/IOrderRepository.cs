using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
 Task<Order?> GetByUserIdAsync(Guid userId);   

 Task<Order?> GetByUserIdWithoutStatusRestrictionAsync(Guid userId);   

 Task<Order> AddAsync(Guid userId);

 Task<Order> ChangeStatusAsync(Guid orderId, OrderStatus newStatus);
}
