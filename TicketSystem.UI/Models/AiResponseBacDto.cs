using System.Text.Json.Serialization;

namespace TicketSystem.UI.Models;

public class AiResponseBacDto<T>
{
    [JsonPropertyName("response")]
    public T? Data { get; set; }

    // Ove dve stvari tvoj bek trenutno ne šalje, pa će biti default vrednosti
    public bool IsSuccess { get; set; } = true;
    public List<string> Errors { get; set; } = [];
}
