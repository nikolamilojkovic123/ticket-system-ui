using TicketSystem.UI.Models.Toast;

namespace TicketSystem.UI.Services;

public class ToastService
{
    public event Action<string, string, ToastType>? OnShowToast;

    public void ShowToast(string title, string message, ToastType type)
    {
        OnShowToast?.Invoke(title, message, type);
    }
}
