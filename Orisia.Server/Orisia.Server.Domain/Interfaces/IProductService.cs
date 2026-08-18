using Orisia.Server.Common.Requests.Product;
using Orisia.Server.Common.Responses.Product;
using Orisia.Server.Core.Pages;

namespace Orisia.Server.Domain.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>?> GetAsync();
    Task<IEnumerable<ProductResponse>?> GetBestSellersAsync(int numOfBestSellers);
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<ProductResponse?> UpdateAsync(UpdateProductRequest request);
    Task<ProductResponse?> CreateAsync(CreateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<Paginated<ProductsResponse>> SearchProductsAsync(SearchProductsRequest request);

}
