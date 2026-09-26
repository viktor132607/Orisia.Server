using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Groups;

public class GroupScheduleRequest
{
    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    [Required, RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d$")]
    public required string StartTime { get; set; }

    [Range(30, 360)]
    public int DurationMinutes { get; set; } = 90;
}
