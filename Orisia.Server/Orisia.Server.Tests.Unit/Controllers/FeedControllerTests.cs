using Microsoft.AspNetCore.Mvc;
using Moq;
using Orisia.Server.API.Controllers;
using Orisia.Server.Common.Responses.Feed;
using Orisia.Server.Domain.Interfaces;
using Xunit;

namespace Orisia.Server.Tests.Unit.Controllers;

public class FeedControllerTests
{
    [Fact]
    public async Task GetFeatured_ShouldUseFeedServiceFeaturedQuery()
    {
        Mock<IFeedService> service = new();
        service
            .Setup(x => x.GetFeaturedAsync(6))
            .ReturnsAsync(new FeedResponse
            {
                Count = 0,
                Items = Array.Empty<FeedItemResponse>()
            });

        FeedController controller = new(service.Object);

        ActionResult<FeedResponse> result = await controller.GetFeatured(6);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(x => x.GetFeaturedAsync(6), Times.Once);
    }
}
