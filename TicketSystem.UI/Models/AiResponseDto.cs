namespace TicketSystem.UI.Models;

public sealed class AiResponseDto
{
    public Guid ConversationId { get; set; }
    public string Type { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string? Content { get; set; }
    public AiActionData Data { get; set; } = default!;
}
