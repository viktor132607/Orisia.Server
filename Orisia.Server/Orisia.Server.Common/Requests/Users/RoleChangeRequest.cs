using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Users;

public class RoleChangeRequest
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required string Role { get; set; }
}
