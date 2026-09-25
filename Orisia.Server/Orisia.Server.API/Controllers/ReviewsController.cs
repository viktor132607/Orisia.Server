using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Reviews;
using Orisia.Server.Common.Responses.Reviews;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SiteReviewResponse>>> GetApproved(
        [FromQuery] bool? featured = null,
        [FromQuery] int take = 20)
    {
        return Ok(await reviewService.GetApprovedAsync(featured, take));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<SiteReviewResponse>> Submit(
        [FromBody] SubmitReviewRequest request)
    {
        SiteReviewResponse created = await reviewService.SubmitAsync(request);
        return Accepted(created);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<SiteReviewResponse>>> GetAdmin(
        [FromQuery] ReviewStatus? status = null)
    {
        return Ok(await reviewService.GetAdminAsync(status));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<SiteReviewResponse>> GetAdminById(Guid id)
    {
        return Ok(await reviewService.GetAdminByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<SiteReviewResponse>> Approve(Guid id)
    {
        return Ok(await reviewService.ApproveAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<SiteReviewResponse>> Reject(Guid id)
    {
        return Ok(await reviewService.RejectAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPatch("{id:guid}/featured")]
    public async Task<ActionResult<SiteReviewResponse>> SetFeatured(
        Guid id,
        [FromBody] SetReviewFeaturedRequest request)
    {
        return Ok(await reviewService.SetFeaturedAsync(id, request.Featured));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await reviewService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
