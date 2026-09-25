using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Events;
using Orisia.Server.Common.Responses.Events;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetPublished(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] EventType? type = null,
        [FromQuery] bool? featured = null)
    {
        return Ok(await eventService.GetPublishedAsync(from, to, type, featured));
    }

    [AllowAnonymous]
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetUpcoming(
        [FromQuery] EventType? type = null,
        [FromQuery] int take = 10)
    {
        return Ok(await eventService.GetUpcomingAsync(type, take));
    }

    [AllowAnonymous]
    [HttpGet("past")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetPast(
        [FromQuery] EventType? type = null,
        [FromQuery] int take = 10)
    {
        return Ok(await eventService.GetPastAsync(type, take));
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<EventResponse>> GetPublishedBySlug(string slug)
    {
        return Ok(await eventService.GetPublishedBySlugAsync(slug));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAdmin(
        [FromQuery] EventType? type = null,
        [FromQuery] PublicationStatus? status = null)
    {
        return Ok(await eventService.GetAdminAsync(type, status));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<EventResponse>> GetAdminById(Guid id)
    {
        return Ok(await eventService.GetAdminByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest request)
    {
        EventResponse created = await eventService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAdminById), new { id = created.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        return Ok(await eventService.UpdateAsync(id, request));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<EventResponse>> Publish(Guid id)
    {
        return Ok(await eventService.PublishAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{id:guid}/unpublish")]
    public async Task<ActionResult<EventResponse>> Unpublish(Guid id)
    {
        return Ok(await eventService.UnpublishAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<EventResponse>> Archive(Guid id)
    {
        return Ok(await eventService.ArchiveAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpPatch("{id:guid}/featured")]
    public async Task<ActionResult<EventResponse>> SetFeatured(
        Guid id,
        [FromBody] SetEventFeaturedRequest request)
    {
        return Ok(await eventService.SetFeaturedAsync(id, request.Featured));
    }

    [Authorize(Policy = AuthorizationPolicies.ContentManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await eventService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
