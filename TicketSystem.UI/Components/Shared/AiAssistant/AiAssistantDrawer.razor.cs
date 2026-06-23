using Microsoft.AspNetCore.Components;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Components.Shared.AiAssistant;

public partial class AiAssistantDrawer
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    [Parameter] public List<ChatMessage> Messages { get; set; } = [];

    [Parameter] public EventCallback<string> OnSend { get; set; }

    private async Task HandleSend(string msg)
    {
        await OnSend.InvokeAsync(msg);
    }

    private Task Close()
    {
        return OnClose.InvokeAsync();
    }
}
