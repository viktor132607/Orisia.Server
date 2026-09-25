using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Auth;

public class DeactivateAccountRequest
{
    [Required]
    public required string CurrentPassword { get; set; }
}
