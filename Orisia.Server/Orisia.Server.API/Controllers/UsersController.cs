using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.API.Helpers;
using Orisia.Server.Common.Requests.Auth;
using Orisia.Server.Common.Requests.Users;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, IAuthService authService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        return await ControllerProcessor.ProcessAsync(() => userService.GetAsync(), this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        return await ControllerProcessor.ProcessAsync(() => userService.GetByIdAsync(id), this);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RegisterUserRequest request)
    {
        return await ControllerProcessor.ProcessAsync(() => authService.RegisterAsync(request), this, true);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route user id does not match payload user id.");
        }

        return await ControllerProcessor.ProcessAsync(() => userService.UpdateAsync(request), this, true);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> SetRoleAsync(Guid id, [FromBody] RoleChangeRequest request)
    {
        if (id != request.UserId)
        {
            return BadRequest("Route user id does not match payload user id.");
        }

        return await ControllerProcessor.ProcessAsync(() => userService.SetRoleAsync(request), this, true);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        return await ControllerProcessor.ProcessAsync<object>(
            async () => await userService.DeleteAsync(id), this);
    }
}
