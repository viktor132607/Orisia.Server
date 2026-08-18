using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Users;

public class RoleChangeRequest
{
    [Required]
    public required Guid UserId { get; set; }
}
