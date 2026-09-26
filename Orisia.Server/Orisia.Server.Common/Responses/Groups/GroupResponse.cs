namespace Orisia.Server.Common.Responses.Groups;

public class GroupResponse
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string NameBg { get; set; }
    public required string NameEn { get; set; }
    public required string DescriptionBg { get; set; }
    public required string DescriptionEn { get; set; }
    public string? Location { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
    public required IReadOnlyCollection<GroupScheduleResponse> Schedules { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}

public class GroupScheduleResponse
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = "00:00";
    public int DurationMinutes { get; set; }
}
