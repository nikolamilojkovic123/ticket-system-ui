namespace TicketSystem.UI.Models;

public class NotificationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Info;
    public Guid? TicketId { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Danger
}
