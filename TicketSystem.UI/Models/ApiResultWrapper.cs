namespace TicketSystem.UI.Models;

public class ApiResultWrapper<T>
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = new();
    public T Data { get; set; } = default!;
}
