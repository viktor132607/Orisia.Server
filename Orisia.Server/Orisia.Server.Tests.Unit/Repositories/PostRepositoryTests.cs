using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class PostRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly PostRepository _repository;

    public PostRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("PostRepositoryTests-" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PostRepository(_context);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnActivePost()
    {
        Post post = CreatePost("hello-world");
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        Post? result = await _repository.GetBySlugAsync("hello-world");

        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldIgnoreSoftDeletedPost()
    {
        Post post = CreatePost("deleted", isDeleted: true);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        Assert.Null(await _repository.GetBySlugAsync("deleted"));
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldSupportExcludingCurrentPost()
    {
        Post post = CreatePost("same-slug");
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        Assert.True(await _repository.SlugExistsAsync("same-slug"));
        Assert.False(await _repository.SlugExistsAsync("same-slug", post.Id));
    }

    [Fact]
    public async Task GetPublishedAsync_ShouldReturnOnlyVisiblePublishedPosts()
    {
        _context.Posts.AddRange(
            CreatePost("published", PublicationStatus.Published, DateTime.UtcNow.AddMinutes(-10), featured: true),
            CreatePost("future", PublicationStatus.Published, DateTime.UtcNow.AddHours(1), featured: true),
            CreatePost("draft", PublicationStatus.Draft, null, featured: true),
            CreatePost("deleted", PublicationStatus.Published, DateTime.UtcNow.AddMinutes(-5), featured: true, isDeleted: true));

        await _context.SaveChangesAsync();

        IEnumerable<Post> result = await _repository.GetPublishedAsync(featured: true);

        Post only = Assert.Single(result);
        Assert.Equal("published", only.Slug);
    }

    private static Post CreatePost(
        string slug,
        PublicationStatus status = PublicationStatus.Draft,
        DateTime? publishedAt = null,
        bool featured = false,
        bool isDeleted = false)
    {
        return new Post
        {
            Slug = slug,
            Type = PostType.News,
            Status = status,
            TitleBg = "Заглавие",
            TitleEn = "Title",
            BodyBg = "Съдържание",
            BodyEn = "Content",
            PublishedAt = publishedAt,
            Featured = featured,
            IsDeleted = isDeleted
        };
    }
}
