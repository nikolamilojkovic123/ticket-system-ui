namespace TicketSystem.UI.Models;

public sealed class ChatMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
}
