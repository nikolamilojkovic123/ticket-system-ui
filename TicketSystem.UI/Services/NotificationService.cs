using TicketSystem.UI.Models;

namespace TicketSystem.UI.Services;

public class NotificationService
{
    private readonly List<NotificationItem> _notifications = new();

    public event Action? OnChange;

    public IReadOnlyList<NotificationItem> Notifications => _notifications.AsReadOnly();
    public int UnreadCount => _notifications.Count(n => !n.IsRead);
    public bool IsPanelOpen { get; private set; }

    // SignalR will call this when real notifications arrive
    public void Push(string title, string message, NotificationType type = NotificationType.Info, Guid? ticketId = null)
    {
        _notifications.Insert(0, new NotificationItem
        {
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.Now,
            TicketId = ticketId
        });
        NotifyStateChanged();
    }

    public void MarkAsRead(Guid id)
    {
        var item = _notifications.FirstOrDefault(n => n.Id == id);
        if (item is { IsRead: false })
        {
            item.IsRead = true;
            NotifyStateChanged();
        }
    }

    public void MarkAllAsRead()
    {
        foreach (var n in _notifications.Where(n => !n.IsRead))
            n.IsRead = true;
        NotifyStateChanged();
    }

    public void Remove(Guid id)
    {
        _notifications.RemoveAll(n => n.Id == id);
        NotifyStateChanged();
    }

    public void ClearAll()
    {
        _notifications.Clear();
        NotifyStateChanged();
    }

    public void OpenPanel() { IsPanelOpen = true; NotifyStateChanged(); }
    public void ClosePanel() { IsPanelOpen = false; NotifyStateChanged(); }
    public void TogglePanel() { IsPanelOpen = !IsPanelOpen; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
