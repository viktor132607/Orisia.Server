using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Review;

public class CreateReviewRequest
{
    [Required]
    public required Guid ProductId { get; set; }
    
    [Required]
    public required string Content { get; set; }
    
    [Required]
    public required byte Rating { get; set; }
}
