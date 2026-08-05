using System.Net.Http.Json;
using System.Text.Json;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Comm;

public static class ApiErrorReader
{
    public static async Task<List<string>> ReadErrorMessagesAsync(HttpResponseMessage response, JsonSerializerOptions options)
    {
        try
        {
            List<ApiErrorDto>? errors = await response.Content.ReadFromJsonAsync<List<ApiErrorDto>>(options);

            if (errors is { Count: > 0 })
                return errors.Select(e => e.Message).ToList();
        }
        catch
        {
        }

        return [$"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"];
    }
}
