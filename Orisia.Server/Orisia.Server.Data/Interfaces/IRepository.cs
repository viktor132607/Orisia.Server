using Orisia.Server.Core.Pages;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.PaginationAndFiltering;

namespace Orisia.Server.Data.Interfaces;

public interface IRepository<TEntity>
    where TEntity : GenericEntity
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<bool> DeleteAsync(Guid id);
    ValueTask<TEntity?> GetByIdAsync(Guid id);
    ValueTask<TEntity?> AddAsync(TEntity? entity);
    ValueTask<TEntity?> UpdateAsync(TEntity? entity);
    Task<Paginated<TEntity>> SearchAsync(Filter<TEntity> filter);
}
