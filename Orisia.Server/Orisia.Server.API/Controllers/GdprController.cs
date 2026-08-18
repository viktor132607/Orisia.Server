using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Responses.Gdpr;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GdprController(IGdprService gdprService) : ControllerBase
{
    [HttpGet("export")]
    public async Task<ActionResult<GdprExportResponse>> ExportData()
    {
        GdprExportResponse response = await gdprService.ExportCurrentUserDataAsync();
        return Ok(response);
    }

    [HttpDelete("delete-account")]
    public async Task<ActionResult<GdprDeleteResponse>> DeleteAccount()
    {
        GdprDeleteResponse response = await gdprService.DeleteCurrentUserDataAsync();
        return Ok(response);
    }
}
