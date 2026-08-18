using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.API.Helpers;
using Orisia.Server.Common.Requests.Category;
using Orisia.Server.Common.Requests.Product;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SearchProductsRequest? request)
    {
        return await ControllerProcessor.ProcessAsync(
            () => productService.SearchProductsAsync(request ?? new SearchProductsRequest()),
            this,
            true);
    }
    
    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellersAsync(int numOfBestSellers)
    {
        return await ControllerProcessor.ProcessAsync(() => productService.GetBestSellersAsync(numOfBestSellers), this, true);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        return await ControllerProcessor.ProcessAsync(() => productService.GetByIdAsync(id), this);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest request)
    {
        return await ControllerProcessor.ProcessAsync(() => productService.CreateAsync(request), this, true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateProductRequest request)
    {
        return await ControllerProcessor.ProcessAsync(() => productService.UpdateAsync(request), this, true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        return await ControllerProcessor.ProcessAsync<object>(
            async () => await productService.DeleteAsync(id), this);
    }
}
