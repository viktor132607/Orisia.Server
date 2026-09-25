using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Orisia.Server.Core.Pages;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Data.PaginationAndFiltering;

namespace Orisia.Server.Data.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity>
    where TEntity : GenericEntity
{
    protected ApplicationDbContext Context { get; } = context;

    public virtual async ValueTask<TEntity?> AddAsync(TEntity? entity)
    {
        if (entity is null)
        {
            return null;
        }

        await Context.Set<TEntity>().AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted)
            .ToListAsync();
    }

    public virtual async ValueTask<TEntity?> GetByIdAsync(Guid id)
    {
        return await Context.Set<TEntity>()
            .FirstOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted);
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        TEntity? entity = await Context.Set<TEntity>().FindAsync(id);
        if (entity is null || entity.IsDeleted)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.ModifiedOn = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        return true;
    }

    public virtual async ValueTask<TEntity?> UpdateAsync(TEntity? entity)
    {
        if (entity is null)
        {
            return null;
        }

        TEntity? currentEntity = await Context.Set<TEntity>()
            .FirstOrDefaultAsync(item => item.Id == entity.Id && !item.IsDeleted);

        if (currentEntity is null)
        {
            return null;
        }

        DateTime createdOn = currentEntity.CreatedOn;
        EntityEntry<TEntity> entry = Context.Entry(currentEntity);
        entry.CurrentValues.SetValues(entity);
        entry.Property(nameof(GenericEntity.CreatedOn)).CurrentValue = createdOn;
        currentEntity.ModifiedOn = DateTime.UtcNow;

        await Context.SaveChangesAsync();
        return currentEntity;
    }

    public async Task<Paginated<TEntity>> SearchAsync(Filter<TEntity> request)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>()
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        foreach (var include in request.Includes)
        {
            query = query.Include(include);
        }

        foreach (string include in request.IncludesAsPropertyPath)
        {
            query = query.Include(include);
        }

        query = query.Where(request.Predicate);

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortDescending == true
                ? query.OrderBy(request.SortBy + " DESC")
                : query.OrderBy(request.SortBy);
        }

        int count = await query.CountAsync();

        if (request.PageNumber is int pageNumber)
        {
            int pageSize = request.PageSize.GetValueOrDefault(10);
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            int skip = (Math.Max(pageNumber, 1) - 1) * pageSize;
            List<TEntity> page = await query.Skip(skip).Take(pageSize).ToListAsync();

            return new Paginated<TEntity> { TotalCount = count, Items = page };
        }

        return new Paginated<TEntity>
        {
            TotalCount = count,
            Items = await query.ToListAsync()
        };
    }
}
