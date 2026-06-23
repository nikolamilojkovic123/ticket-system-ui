namespace TicketSystem.UI.Models;

public sealed class DashboardResponse
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageSeverity { get; set; }
    public int CriticalCount { get; set; }
}
