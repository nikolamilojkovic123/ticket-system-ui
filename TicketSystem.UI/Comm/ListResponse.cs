namespace TicketSystem.UI.Comm;

public class ListResponse<T>
{
    public ICollection<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
