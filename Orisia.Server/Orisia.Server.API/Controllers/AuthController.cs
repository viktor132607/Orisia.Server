using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orisia.Server.Common.Requests.Auth;
using Orisia.Server.Common.Requests.Users;
using Orisia.Server.Common.Responses.Auth;
using Orisia.Server.Common.Responses.Users;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserRequest request)
    {
        RegisterUserResponse? user = await authService.RegisterAsync(request);

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginUserRequest request)
    {
        TokenResponse? result = await authService.LoginAsync(request);
        if (result is null)
        {
            return BadRequest("Invalid username or password.");
        }

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request)
    {
        ForgotPasswordResponse result = await authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        bool result = await authService.ResetPasswordAsync(request);
        return result ? Ok() : BadRequest();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        return await authService.ChangePasswordAsync(request)
            ? NoContent()
            : BadRequest();
    }

    [Authorize]
    [HttpPost("me/deactivate")]
    public async Task<IActionResult> DeactivateCurrentAccount(DeactivateAccountRequest request)
    {
        return await authService.DeactivateCurrentAccountAsync(request)
            ? NoContent()
            : BadRequest();
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult<TokenResponse>> Logout()
    {
        if (!await authService.LogoutAsync())
        {
            return Unauthorized();
        }

        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        TokenResponse? result = await authService.RefreshTokensAsync(request);
        if (result is null || result.AccessToken is null || result.RefreshToken is null)
        {
            return Unauthorized("Invalid refresh token.");
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        UserResponse? user = await userService.GetCurrentUserAsync();
        return Ok(user);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateCurrentUser(UpdateCurrentUserRequest request)
    {
        UserResponse? user = await userService.UpdateCurrentUserAsync(request);
        return Ok(user);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated!");
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are an admin!");
    }
}
