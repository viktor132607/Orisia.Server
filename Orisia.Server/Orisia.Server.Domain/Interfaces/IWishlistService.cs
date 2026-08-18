using Orisia.Server.Common.Responses.Wishlist;

namespace Orisia.Server.Domain.Interfaces;

public interface IWishlistService
{
    Task<WishlistResponse> GetByJWT();
    Task<bool> AddProductToWishlistAsync(Guid productId);
    Task<bool> RemoveProductFromWishlistAsync(Guid productId);
}
