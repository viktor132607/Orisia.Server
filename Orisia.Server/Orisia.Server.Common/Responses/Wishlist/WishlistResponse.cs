using Orisia.Server.Common.Responses.Product;

namespace Orisia.Server.Common.Responses.Wishlist;

public class WishlistResponse
{
    public ICollection<ProductsResponse> Products { get; set; } = new List<ProductsResponse>();
}
