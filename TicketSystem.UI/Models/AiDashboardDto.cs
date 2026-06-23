namespace TicketSystem.UI.Models;

public sealed class AiDashboardDto
{
    public double AverageSeverity { get; set; }
    public int CriticalCount { get; set; }

    public List<AiKeywordDto> TopKeywords { get; set; } = new();
    public List<AiTicketDto> CriticalTickets { get; set; } = new();
}
