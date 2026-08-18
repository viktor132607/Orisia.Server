using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Users;

public class UpdateCurrentUserRequest
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Names { get; set; }

    [Required]
    public required string Phone { get; set; }
}
