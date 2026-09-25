using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Gallery;
using Orisia.Server.Common.Responses.Gallery;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GalleryController(IGalleryService galleryService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GalleryAlbumResponse>>> GetPublicAlbums(
        [FromQuery] bool? featured = null)
    {
        return Ok(await galleryService.GetPublicAlbumsAsync(featured));
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<GalleryAlbumResponse>> GetPublicAlbumBySlug(string slug)
    {
        return Ok(await galleryService.GetPublicAlbumBySlugAsync(slug));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<GalleryAlbumResponse>>> GetAdminAlbums()
    {
        return Ok(await galleryService.GetAdminAlbumsAsync());
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<GalleryAlbumResponse>> GetAdminAlbumById(Guid id)
    {
        return Ok(await galleryService.GetAdminAlbumByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost]
    public async Task<ActionResult<GalleryAlbumResponse>> CreateAlbum(
        [FromBody] CreateGalleryAlbumRequest request)
    {
        GalleryAlbumResponse created = await galleryService.CreateAlbumAsync(request);
        return CreatedAtAction(nameof(GetAdminAlbumById), new { id = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GalleryAlbumResponse>> UpdateAlbum(
        Guid id,
        [FromBody] UpdateGalleryAlbumRequest request)
    {
        return Ok(await galleryService.UpdateAlbumAsync(id, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAlbum(Guid id)
    {
        return await galleryService.DeleteAlbumAsync(id) ? NoContent() : NotFound();
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{albumId:guid}/media")]
    public async Task<ActionResult<IReadOnlyCollection<GalleryMediaResponse>>> AddMedia(
        Guid albumId,
        [FromBody] AddGalleryMediaRequest request)
    {
        return Ok(await galleryService.AddMediaAsync(albumId, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("media/{galleryMediaId:guid}")]
    public async Task<ActionResult<GalleryMediaResponse>> UpdateMedia(
        Guid galleryMediaId,
        [FromBody] UpdateGalleryMediaRequest request)
    {
        return Ok(await galleryService.UpdateMediaAsync(galleryMediaId, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("media/{galleryMediaId:guid}/move")]
    public async Task<ActionResult<GalleryMediaResponse>> MoveMedia(
        Guid galleryMediaId,
        [FromBody] MoveGalleryMediaRequest request)
    {
        return Ok(await galleryService.MoveMediaAsync(galleryMediaId, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{albumId:guid}/reorder")]
    public async Task<IActionResult> Reorder(
        Guid albumId,
        [FromBody] ReorderGalleryMediaRequest request)
    {
        await galleryService.ReorderMediaAsync(albumId, request);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("media/{galleryMediaId:guid}")]
    public async Task<IActionResult> DeleteMedia(Guid galleryMediaId)
    {
        return await galleryService.DeleteMediaAsync(galleryMediaId)
            ? NoContent()
            : NotFound();
    }
}
