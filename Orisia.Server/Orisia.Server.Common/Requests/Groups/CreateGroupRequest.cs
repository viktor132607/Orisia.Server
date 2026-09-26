using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Groups;

public class CreateGroupRequest
{
    [MaxLength(180)]
    public string? Slug { get; set; }

    [Required, MaxLength(250)]
    public required string NameBg { get; set; }

    [Required, MaxLength(250)]
    public required string NameEn { get; set; }

    [Required]
    public required string DescriptionBg { get; set; }

    [Required]
    public required string DescriptionEn { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public bool Active { get; set; } = true;
    public int SortOrder { get; set; }

    [MinLength(1)]
    public List<GroupScheduleRequest> Schedules { get; set; } = [];
}
