using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.API.Models.Media;
using Orisia.Server.Common.Responses.Media;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Media;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.ContentManagement)]
[Route("api/admin/media")]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MediaResponse>>> GetAll()
    {
        return Ok(await mediaService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaResponse>> GetById(Guid id)
    {
        return Ok(await mediaService.GetByIdAsync(id));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<MediaResponse>> Upload(
        [FromForm] UploadMediaForm form,
        CancellationToken cancellationToken)
    {
        await using Stream stream = form.File.OpenReadStream();

        MediaResponse created = await mediaService.UploadAsync(
            new MediaUploadInput(
                stream,
                form.File.FileName,
                form.File.ContentType,
                form.File.Length,
                form.AltBg,
                form.AltEn),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await mediaService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
