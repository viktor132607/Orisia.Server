using Orisia.Server.Common.Requests.Category;
using Orisia.Server.Common.Responses.Category;

namespace Orisia.Server.Domain.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>?> GetAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> UpdateAsync(UpdateCategoryRequest request);
    Task<CategoryResponse?> CreateAsync(CreateCategoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}
