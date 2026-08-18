using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.API.Helpers;
using Orisia.Server.Common.Requests.Wishlist;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WishlistController(IWishlistService wishlistService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        return await ControllerProcessor.ProcessAsync(() => wishlistService.GetByJWT(), this);
    }
    
    [HttpPost("add-product")]
    public async Task<IActionResult> AddProductToWhishlistAsync([FromBody] AddToWishlistRequest request)
    {
        return await ControllerProcessor.ProcessAsync<object>(
            async () => await wishlistService.AddProductToWishlistAsync(request.ProductId), this);    
    }
    
    [HttpDelete("remove-product")]
    public async Task<IActionResult> RemoveProductFromWhishlistAsync([FromBody] RemoveFromWishlistRequest request)
    {
        return await ControllerProcessor.ProcessAsync<object>(
            async () => await wishlistService.RemoveProductFromWishlistAsync(request.ProductId), this); 
    }
}
