using System.ComponentModel.DataAnnotations;

namespace Orisia.Server.Common.Requests.Horoteka;

public class ReorderDancesRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyCollection<DanceOrderItem> Items { get; set; }
}

public class DanceOrderItem
{
    public Guid DanceId { get; set; }
    public int SortOrder { get; set; }
}
