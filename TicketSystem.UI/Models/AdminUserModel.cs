using TicketSystem.UI.Enums;

namespace TicketSystem.UI.Models;

public sealed class AdminUserModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
}
