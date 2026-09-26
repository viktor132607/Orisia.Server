namespace Orisia.Server.Data.Entities;

public class DanceGroup : GenericEntity
{
    public required string Slug { get; set; }
    public required string NameBg { get; set; }
    public required string NameEn { get; set; }
    public required string DescriptionBg { get; set; }
    public required string DescriptionEn { get; set; }
    public string? Location { get; set; }
    public bool Active { get; set; } = true;
    public int SortOrder { get; set; }
    public ICollection<DanceGroupSchedule> Schedules { get; set; } = new List<DanceGroupSchedule>();
}
