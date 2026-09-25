using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Responses.Posts;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController(IPostService postService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPublished(
        [FromQuery] bool? featured = null,
        [FromQuery] int? take = null)
    {
        IEnumerable<PostResponse> posts = await postService.GetPublishedAsync(
            PostType.Blog,
            featured,
            take);

        return Ok(posts);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostResponse>> GetBySlug(string slug)
    {
        PostResponse post = await postService.GetPublishedBySlugAsync(slug);
        return post.Type == PostType.Blog ? Ok(post) : NotFound();
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetAdmin(
        [FromQuery] PublicationStatus? status = null)
    {
        IEnumerable<PostResponse> posts = await postService.GetAdminAsync(
            PostType.Blog,
            status);

        return Ok(posts);
    }
}
