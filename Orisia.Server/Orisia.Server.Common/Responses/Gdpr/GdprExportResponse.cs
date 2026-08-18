using Orisia.Server.Common.Responses.Order;
using Orisia.Server.Common.Responses.Review;
using Orisia.Server.Common.Responses.Users;

namespace Orisia.Server.Common.Responses.Gdpr;

public class GdprExportResponse
{
    public DateTime RequestedAtUtc { get; set; }

    public UserResponse? User { get; set; }

    public ICollection<OrderResponse> Orders { get; set; } = new List<OrderResponse>();

    public ICollection<Guid> WishlistProductIds { get; set; } = new List<Guid>();

    public ICollection<ReviewResponse> Reviews { get; set; } = new List<ReviewResponse>();
}
