using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Inquiries;

public class AnswerInquiryRequest
{
    [Required, MinLength(2), MaxLength(5000)]
    public required string Answer { get; set; }
}
