namespace Orisia.Server.Data.Entities;

public class DanceGroupSchedule : GenericEntity
{
    public Guid DanceGroupId { get; set; }
    public DanceGroup? DanceGroup { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; } = 90;
}
