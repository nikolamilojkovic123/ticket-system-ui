using Microsoft.AspNetCore.Components;
using TicketSystem.UI.Models;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Shared.Notifications;

public partial class NotificationPanel : IDisposable
{
    [Inject]
    public NotificationService NotificationService { get; set; } = default!;

    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    protected override void OnInitialized()
    {
        NotificationService.OnChange += HandleChange;
    }

    private void HandleChange() => InvokeAsync(StateHasChanged);

    private void HandleItemClick(NotificationItem item)
    {
        NotificationService.MarkAsRead(item.Id);
        if (item.TicketId.HasValue)
        {
            NotificationService.ClosePanel();
            Nav.NavigateTo($"/ticket/edit/{item.TicketId.Value}");
        }
    }

    private static string GetTypeIcon(NotificationType type) => type switch
    {
        NotificationType.Success => "bi-check-circle-fill",
        NotificationType.Warning => "bi-exclamation-triangle-fill",
        NotificationType.Danger  => "bi-x-circle-fill",
        _                        => "bi-info-circle-fill"
    };

    private static string FormatTime(DateTime time)
    {
        var diff = DateTime.Now - time;
        if (diff.TotalMinutes < 1)  return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)   return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7)     return $"{(int)diff.TotalDays}d ago";
        return time.ToString("MMM dd");
    }

    public void Dispose()
    {
        NotificationService.OnChange -= HandleChange;
    }
}
