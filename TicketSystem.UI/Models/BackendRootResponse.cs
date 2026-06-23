namespace TicketSystem.UI.Models;

public sealed class BackendRootResponse<T>
{
    public T? Response { get; set; }
}
