using Microsoft.AspNetCore.Mvc;
using Moq;
using Orisia.Server.API.Controllers;
using Orisia.Server.Common.Responses.Posts;
using Orisia.Server.Core.Enums;
using Orisia.Server.Domain.Interfaces;
using Xunit;

namespace Orisia.Server.Tests.Unit.Controllers;

public class BlogControllerTests
{
    [Fact]
    public async Task GetPublished_ShouldForceBlogType()
    {
        Mock<IPostService> service = new();
        service
            .Setup(x => x.GetPublishedAsync(PostType.Blog, true, 3))
            .ReturnsAsync(Array.Empty<PostResponse>());

        BlogController controller = new(service.Object);

        ActionResult<IEnumerable<PostResponse>> result = await controller.GetPublished(true, 3);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(
            x => x.GetPublishedAsync(PostType.Blog, true, 3),
            Times.Once);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnNotFound_WhenPostIsNotBlog()
    {
        Mock<IPostService> service = new();
        service
            .Setup(x => x.GetPublishedBySlugAsync("news-item"))
            .ReturnsAsync(CreateResponse(PostType.News));

        BlogController controller = new(service.Object);

        ActionResult<PostResponse> result = await controller.GetBySlug("news-item");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private static PostResponse CreateResponse(PostType type)
    {
        return new PostResponse
        {
            Id = Guid.NewGuid(),
            Slug = "news-item",
            Type = type,
            Status = PublicationStatus.Published,
            TitleBg = "Заглавие",
            TitleEn = "Title",
            BodyBg = "Текст",
            BodyEn = "Text",
            SeoTitleBg = "Заглавие",
            SeoTitleEn = "Title",
            SeoDescriptionBg = "Текст",
            SeoDescriptionEn = "Text"
        };
    }
}
