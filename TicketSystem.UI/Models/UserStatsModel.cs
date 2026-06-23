namespace TicketSystem.UI.Models;

public sealed class UserStatsModel
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int Total { get; set; }
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ClosedCount { get; set; }
    public double? AvgResolutionDays { get; set; }

    public double ResolutionRate => Total > 0 ? (double)ClosedCount / Total * 100 : 0;
}
