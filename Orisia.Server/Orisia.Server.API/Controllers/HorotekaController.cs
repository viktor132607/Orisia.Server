using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Horoteka;
using Orisia.Server.Common.Responses.Horoteka;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HorotekaController(IDanceService danceService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DanceResponse>>> GetPublic(
        [FromQuery] string? region = null)
    {
        return Ok(await danceService.GetPublicAsync(region));
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<DanceResponse>> GetPublicBySlug(string slug)
    {
        return Ok(await danceService.GetPublicBySlugAsync(slug));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<DanceResponse>>> GetAdmin()
    {
        return Ok(await danceService.GetAdminAsync());
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<DanceResponse>> GetAdminById(Guid id)
    {
        return Ok(await danceService.GetAdminByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost]
    public async Task<ActionResult<DanceResponse>> Create(
        [FromBody] CreateDanceRequest request)
    {
        DanceResponse created = await danceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAdminById), new { id = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DanceResponse>> Update(
        Guid id,
        [FromBody] UpdateDanceRequest request)
    {
        return Ok(await danceService.UpdateAsync(id, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderDancesRequest request)
    {
        await danceService.ReorderAsync(request);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await danceService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
