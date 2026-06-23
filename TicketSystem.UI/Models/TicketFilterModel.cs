using TicketSystem.UI.Enums;

namespace TicketSystem.UI.Models;

public sealed class TicketFilterModel
{
    public HashSet<TicketStatus>   Statuses   { get; set; } = new();
    public HashSet<TicketPriority> Priorities { get; set; } = new();
    public HashSet<TicketCategory> Categories { get; set; } = new();
    public Guid?     AssigneeId { get; set; }
    public DateTime? DateFrom   { get; set; }
    public DateTime? DateTo     { get; set; }

    public bool IsEmpty =>
        Statuses.Count == 0 && Priorities.Count == 0 && Categories.Count == 0
        && AssigneeId is null && DateFrom is null && DateTo is null;

    public int ActiveCount =>
        Statuses.Count + Priorities.Count + Categories.Count
        + (AssigneeId.HasValue ? 1 : 0)
        + (DateFrom.HasValue   ? 1 : 0)
        + (DateTo.HasValue     ? 1 : 0);
}
