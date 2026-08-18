using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Auth;

public class RegisterUserRequest
{
    [Required]
    public required string Email { get; set; } 
    
    [Required]
    public required string Password { get; set; } 
    
    [Required]
    public required string Names { get; set; } 
    
    [Required]
    public required string Phone { get; set; } 
}
