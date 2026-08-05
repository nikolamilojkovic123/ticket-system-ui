namespace TicketSystem.UI.Models;

public sealed class ApiErrorDto
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
}
