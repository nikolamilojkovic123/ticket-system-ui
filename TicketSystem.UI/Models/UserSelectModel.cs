namespace TicketSystem.UI.Models;

public sealed class UserSelectModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
}
