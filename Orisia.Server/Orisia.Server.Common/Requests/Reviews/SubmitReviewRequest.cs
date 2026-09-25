using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Reviews;

public class SubmitReviewRequest
{
    [Required, MinLength(2), MaxLength(120)]
    public required string AuthorName { get; set; }

    [Required, MinLength(10), MaxLength(2000)]
    public required string Content { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }
}
