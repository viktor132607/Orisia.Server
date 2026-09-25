using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class PostRepository(ApplicationDbContext context)
    : Repository<Post>(context), IPostRepository
{
    public async Task<Post?> GetBySlugAsync(string slug)
    {
        return await Context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .FirstOrDefaultAsync(post => post.Slug == slug && !post.IsDeleted);
    }

    public async Task<Post?> GetByIdWithAuthorAsync(Guid id)
    {
        return await Context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .FirstOrDefaultAsync(post => post.Id == id && !post.IsDeleted);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludingPostId = null)
    {
        return await Context.Posts.AnyAsync(post =>
            post.Slug == slug
            && !post.IsDeleted
            && (!excludingPostId.HasValue || post.Id != excludingPostId.Value));
    }

    public async Task<IEnumerable<Post>> GetPublishedAsync(
        PostType? type = null,
        bool? featured = null,
        int? take = null)
    {
        DateTime now = DateTime.UtcNow;

        IQueryable<Post> query = Context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .Where(post =>
                !post.IsDeleted
                && post.Status == PublicationStatus.Published
                && post.PublishedAt.HasValue
                && post.PublishedAt.Value <= now);

        if (type.HasValue)
        {
            query = query.Where(post => post.Type == type.Value);
        }

        if (featured.HasValue)
        {
            query = query.Where(post => post.Featured == featured.Value);
        }

        query = query
            .OrderByDescending(post => post.PublishedAt)
            .ThenByDescending(post => post.CreatedOn);

        if (take is > 0)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetForAdminAsync(
        PostType? type = null,
        PublicationStatus? status = null)
    {
        IQueryable<Post> query = Context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .Where(post => !post.IsDeleted);

        if (type.HasValue)
        {
            query = query.Where(post => post.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(post => post.Status == status.Value);
        }

        return await query
            .OrderByDescending(post => post.ModifiedOn)
            .ThenByDescending(post => post.CreatedOn)
            .ToListAsync();
    }
}
