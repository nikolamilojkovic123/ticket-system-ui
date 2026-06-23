using TicketSystem.UI.Enums;

namespace TicketSystem.UI.Models;

public sealed class CreateTicketModel
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketCategory Category { get; set; } = TicketCategory.Software;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public Guid? UserId { get; set; }
}
