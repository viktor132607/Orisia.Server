using Microsoft.AspNetCore.Mvc;
using Moq;
using Orisia.Server.API.Controllers;
using Orisia.Server.Common.Responses.Admin;
using Orisia.Server.Domain.Interfaces;
using Xunit;

namespace Orisia.Server.Tests.Unit.Controllers;

public class AdminDashboardControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnDashboardPayload()
    {
        AdminDashboardResponse payload = new()
        {
            GeneratedAt = DateTime.UtcNow,
            Users = new UserDashboardMetrics(),
            Posts = new PostDashboardMetrics(),
            Events = new EventDashboardMetrics(),
            Gallery = new GalleryDashboardMetrics(),
            Reviews = new ReviewDashboardMetrics(),
            Inquiries = new InquiryDashboardMetrics(),
            Media = new MediaDashboardMetrics(),
            Dances = new DanceDashboardMetrics(),
            RecentPosts = Array.Empty<DashboardPostActivity>(),
            UpcomingEvents = Array.Empty<DashboardEventActivity>(),
            RecentInquiries = Array.Empty<DashboardInquiryActivity>(),
            PendingReviews = Array.Empty<DashboardReviewActivity>()
        };

        Mock<IAdminDashboardService> service = new();
        service.Setup(x => x.GetAsync()).ReturnsAsync(payload);

        AdminDashboardController controller = new(service.Object);

        ActionResult<AdminDashboardResponse> result = await controller.Get();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(payload, ok.Value);
    }
}
