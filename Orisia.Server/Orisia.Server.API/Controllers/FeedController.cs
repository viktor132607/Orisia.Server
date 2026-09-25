using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Responses.Feed;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class FeedController(IFeedService feedService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FeedResponse>> Get(
        [FromQuery] string? type = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] bool? featured = null,
        [FromQuery] int take = 20)
    {
        return Ok(await feedService.GetAsync(type, from, to, featured, take));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<FeedResponse>> GetLatest(
        [FromQuery] int take = 20)
    {
        return Ok(await feedService.GetLatestAsync(take));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<FeedResponse>> GetFeatured(
        [FromQuery] int take = 8)
    {
        return Ok(await feedService.GetFeaturedAsync(take));
    }
}
