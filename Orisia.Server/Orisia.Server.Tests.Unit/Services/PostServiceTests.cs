using Moq;
using Orisia.Server.Common.Requests.Posts;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IAuthService> _auth = new();
    private readonly PostService _service;

    public PostServiceTests()
    {
        _service = new PostService(_posts.Object, _auth.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateSlugAndCreateDraft()
    {
        Guid authorId = Guid.NewGuid();
        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(authorId.ToString());
        _posts.Setup(x => x.SlugExistsAsync("nova-statiya", null)).ReturnsAsync(false);
        _posts.Setup(x => x.AddAsync(It.IsAny<Post>()))
            .ReturnsAsync((Post post) => post);
        _posts.Setup(x => x.GetByIdWithAuthorAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Post
            {
                Id = id,
                Slug = "nova-statiya",
                Type = PostType.Blog,
                Status = PublicationStatus.Draft,
                TitleBg = "Нова статия",
                TitleEn = "Nova statiya",
                BodyBg = "Текст",
                BodyEn = "Text",
                AuthorId = authorId
            });

        var result = await _service.CreateAsync(new CreatePostRequest
        {
            Type = PostType.Blog,
            TitleBg = "Нова статия",
            TitleEn = "Nova statiya",
            BodyBg = "Текст",
            BodyEn = "Text"
        });

        Assert.Equal("nova-statiya", result.Slug);
        Assert.Equal(PublicationStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_ShouldRejectDraft()
    {
        _posts.Setup(x => x.GetBySlugAsync("draft"))
            .ReturnsAsync(new Post
            {
                Slug = "draft",
                Type = PostType.News,
                Status = PublicationStatus.Draft,
                TitleBg = "Чернова",
                TitleEn = "Draft",
                BodyBg = "Текст",
                BodyEn = "Text"
            });

        await Assert.ThrowsAsync<AppException>(() => _service.GetPublishedBySlugAsync("draft"));
    }

    [Fact]
    public async Task PublishAsync_ShouldSetStatusAndPublishedAt()
    {
        Guid id = Guid.NewGuid();
        Post post = CreatePost(id, PublicationStatus.Draft);

        _posts.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(post);
        _posts.Setup(x => x.UpdateAsync(It.IsAny<Post>()))
            .ReturnsAsync((Post updated) => updated);
        _posts.Setup(x => x.GetByIdWithAuthorAsync(id))
            .ReturnsAsync((Guid _) =>
            {
                post.Status = PublicationStatus.Published;
                post.PublishedAt = DateTime.UtcNow;
                return post;
            });

        var result = await _service.PublishAsync(id);

        Assert.Equal(PublicationStatus.Published, result.Status);
        Assert.NotNull(result.PublishedAt);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectDuplicateSlug()
    {
        Guid id = Guid.NewGuid();
        _posts.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(CreatePost(id));
        _posts.Setup(x => x.SlugExistsAsync("duplicate", id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(id, new UpdatePostRequest
        {
            Slug = "duplicate",
            Type = PostType.News,
            TitleBg = "Заглавие",
            TitleEn = "Title",
            BodyBg = "Текст",
            BodyEn = "Text"
        }));
    }

    [Fact]
    public async Task GetPublishedAsync_ShouldRejectInvalidTake()
    {
        await Assert.ThrowsAsync<AppException>(() => _service.GetPublishedAsync(take: 0));
    }

    private static Post CreatePost(Guid id, PublicationStatus status = PublicationStatus.Draft)
    {
        return new Post
        {
            Id = id,
            Slug = "post",
            Type = PostType.News,
            Status = status,
            TitleBg = "Заглавие",
            TitleEn = "Title",
            BodyBg = "Текст",
            BodyEn = "Text"
        };
    }
}
