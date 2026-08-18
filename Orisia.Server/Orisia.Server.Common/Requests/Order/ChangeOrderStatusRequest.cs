using System.ComponentModel.DataAnnotations;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Requests.Order;

public class ChangeOrderStatusRequest
{
    [Required]
    public required Guid OrderId { get; set; }
    
    [Required]
    public required OrderStatus OrderStatus { get; set; }
}
