using Orisia.Server.Common.Requests.Auth;
using Orisia.Server.Common.Responses.Auth;

namespace Orisia.Server.Domain.Interfaces;

public interface IAuthService
{
    Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request);
    Task<TokenResponse?> LoginAsync(LoginUserRequest request);
    Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequest request);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> DeactivateCurrentAccountAsync(DeactivateAccountRequest request);
    Task<string?> GetCurrentUserRole();
    Task<string?> GetCurrentUserEmail();
    Task<string?> GetCurrentUserId();
    Task<bool> LogoutAsync();
}
