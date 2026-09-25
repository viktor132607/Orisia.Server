using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Users;

public class UpdateUserRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; set; }

    [Required, MinLength(2), MaxLength(120)]
    public required string Names { get; set; }

    [Required, MaxLength(50)]
    public required string Phone { get; set; }
}
