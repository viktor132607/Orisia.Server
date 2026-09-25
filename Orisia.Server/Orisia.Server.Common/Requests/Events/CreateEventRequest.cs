using System.ComponentModel.DataAnnotations;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Common.Requests.Events;

public class CreateEventRequest
{
    [MaxLength(180)]
    public string? Slug { get; set; }

    [Required, MaxLength(250)]
    public required string TitleBg { get; set; }

    [Required, MaxLength(250)]
    public required string TitleEn { get; set; }

    [Required]
    public required string DescriptionBg { get; set; }

    [Required]
    public required string DescriptionEn { get; set; }

    [Required]
    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset? EndAt { get; set; }

    public bool AllDay { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public Guid? CoverMediaId { get; set; }

    public bool Featured { get; set; }

    [MaxLength(500)]
    public string? RecurrenceRule { get; set; }
}
