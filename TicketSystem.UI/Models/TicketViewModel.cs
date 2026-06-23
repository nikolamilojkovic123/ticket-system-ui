using TicketSystem.UI.Enums;

namespace TicketSystem.UI.Models;

public sealed class TicketViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public TicketCategory Category { get; init; }
    public TicketPriority Priority { get; init; }
    public TicketStatus Status { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = default!;
}
