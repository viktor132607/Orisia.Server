using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Inquiries;
using Orisia.Server.Common.Responses.Inquiries;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController(IInquiryService inquiryService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<ContactInquiryResponse>> Submit(
        [FromBody] CreateInquiryRequest request)
    {
        ContactInquiryResponse created = await inquiryService.SubmitAsync(request);
        return Accepted(created);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<ContactInquiryResponse>>> GetAdmin(
        [FromQuery] InquiryStatus? status = null)
    {
        return Ok(await inquiryService.GetAdminAsync(status));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<ContactInquiryResponse>> GetAdminById(Guid id)
    {
        return Ok(await inquiryService.GetAdminByIdAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ContactInquiryResponse>> MarkRead(Guid id)
    {
        return Ok(await inquiryService.MarkReadAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/answer")]
    public async Task<ActionResult<ContactInquiryResponse>> Answer(
        Guid id,
        [FromBody] AnswerInquiryRequest request)
    {
        return Ok(await inquiryService.AnswerAsync(id, request));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ContactInquiryResponse>> Archive(Guid id)
    {
        return Ok(await inquiryService.ArchiveAsync(id));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await inquiryService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
