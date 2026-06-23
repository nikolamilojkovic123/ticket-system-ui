using Microsoft.AspNetCore.Components;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Components.Shared.AiAssistant;

public partial class AiMessageBubble
{
    [Parameter] public ChatMessage Message { get; set; }
}
