using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Image;

public class CreateImageRequest
{
    [Required]
    public required string Uri { get; set; }
}
