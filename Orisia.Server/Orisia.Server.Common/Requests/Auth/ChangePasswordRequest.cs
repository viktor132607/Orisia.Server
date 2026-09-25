using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Auth;

public class ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8), MaxLength(200)]
    public required string NewPassword { get; set; }
}
