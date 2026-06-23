namespace TicketSystem.UI.Models;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = [];

    public T? Data { get; set; }
}