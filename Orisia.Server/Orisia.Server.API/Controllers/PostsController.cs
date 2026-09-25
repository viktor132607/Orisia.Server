using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Posts;
using Orisia.Server.Common.Responses.Posts;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController(IPostService postService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPublished(
        [FromQuery] PostType? type = null,
        [FromQuery] bool? featured = null,
        [FromQuery] int? take = null)
    {
        IEnumerable<PostResponse> posts = await postService.GetPublishedAsync(type, featured, take);
        return Ok(posts);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostResponse>> GetPublishedBySlug(string slug)
    {
        return Ok(await postService.GetPublishedBySlugAsync(slug));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetAdmin(
        [FromQuery] PostType? type = null,
        [FromQuery] PublicationStatus? status = null)
    {
        IEnumerable<PostResponse> posts = await postService.GetAdminAsync(type, status);
        return Ok(posts);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<PostResponse>> GetAdminById(Guid id)
    {
        return Ok(await postService.GetAdminByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost]
    public async Task<ActionResult<PostResponse>> Create([FromBody] CreatePostRequest request)
    {
        PostResponse created = await postService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAdminById), new { id = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostResponse>> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        return Ok(await postService.UpdateAsync(id, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PostResponse>> Publish(Guid id)
    {
        return Ok(await postService.PublishAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<PostResponse>> Archive(Guid id)
    {
        return Ok(await postService.ArchiveAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPatch("{id:guid}/featured")]
    public async Task<ActionResult<PostResponse>> SetFeatured(Guid id, [FromBody] SetFeaturedRequest request)
    {
        return Ok(await postService.SetFeaturedAsync(id, request.Featured));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await postService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
