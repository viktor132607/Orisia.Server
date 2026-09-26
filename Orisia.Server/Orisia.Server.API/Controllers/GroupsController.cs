using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Groups;
using Orisia.Server.Common.Responses.Groups;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController(IGroupService groupService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetPublic() =>
        Ok(await groupService.GetPublicAsync());

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<GroupResponse>> GetPublicBySlug(string slug) =>
        Ok(await groupService.GetPublicBySlugAsync(slug));

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetAdmin() =>
        Ok(await groupService.GetAdminAsync());

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<GroupResponse>> GetAdminById(Guid id) =>
        Ok(await groupService.GetAdminByIdAsync(id));

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost]
    public async Task<ActionResult<GroupResponse>> Create([FromBody] CreateGroupRequest request)
    {
        GroupResponse created = await groupService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAdminById), new { id = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GroupResponse>> Update(Guid id, [FromBody] UpdateGroupRequest request) =>
        Ok(await groupService.UpdateAsync(id, request));

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await groupService.DeleteAsync(id) ? NoContent() : NotFound();
}
