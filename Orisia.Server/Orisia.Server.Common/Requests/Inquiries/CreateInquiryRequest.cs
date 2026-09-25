using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Inquiries;

public class CreateInquiryRequest
{
    [Required, MinLength(2), MaxLength(120)]
    public required string Name { get; set; }

    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; set; }

    [Phone, MaxLength(50)]
    public string? Phone { get; set; }

    [Required, MinLength(3), MaxLength(250)]
    public required string Subject { get; set; }

    [Required, MinLength(10), MaxLength(5000)]
    public required string Message { get; set; }
}
