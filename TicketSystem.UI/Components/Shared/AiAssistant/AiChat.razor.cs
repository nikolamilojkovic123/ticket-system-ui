using Microsoft.AspNetCore.Components;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Components.Shared.AiAssistant;

public partial class AiChat
{
    [Parameter] public List<ChatMessage> Messages { get; set; } = [];
    [Parameter] public EventCallback<string> OnSend { get; set; }

    private string input;

    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        string msg = input;
        input = string.Empty;

        await OnSend.InvokeAsync(msg);
    }
}
