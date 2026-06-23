namespace TicketSystem.UI.Models;

public class TicketCommentDto
{
    public Guid Id { get; set; }
    public string Author { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
