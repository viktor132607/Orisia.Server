using Orisia.Server.Common.Requests.Order;
using Orisia.Server.Common.Requests.OrderItem;
using Orisia.Server.Common.Responses.Order;
using Orisia.Server.Core.Pages;

namespace Orisia.Server.Domain.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> GetAsync();
    Task<OrderResponse> AddProductAsync(AddOrderItemRequest product);
    Task<OrderResponse> RemoveProductAsync(RemoveOrderItemRequest product);
    Task<bool> SendCurrentAsync(SendOrderRequest request);
    Task<bool> ChangeStatusAsync(ChangeOrderStatusRequest request);
    Task<Paginated<OrderResponse>> SearchOrdersAsync(SearchOrderRequest request);
}
 
