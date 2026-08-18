using Orisia.Server.Common.Responses.OrderItem;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;

namespace Orisia.Server.Common.Responses.Order;

public class OrderResponse
{
    public required Guid Id { get; set; }
    public required decimal OrderTotalPrice { get; set; }
    public string CurrencyCode { get; set; } = CurrencyDefaults.Code;
    public Guid UserId { get; set; }
    public string? Names { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public ICollection<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
}
