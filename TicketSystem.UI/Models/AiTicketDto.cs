namespace TicketSystem.UI.Models;

public sealed class AiTicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string AiSummary { get; set; } = "";
    public int SeverityScore { get; set; }
}
