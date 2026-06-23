using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace TicketSystem.UI.Services;

public class DashboardService(IHttpClientFactory factory, ILogger<DashboardService> logger) : IDashboardService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient = factory.CreateClient("BackendAPI");

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/dashboard");

            if (response.IsSuccessStatusCode)
            {
                ApiResultWrapper<DashboardResponse>? result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<DashboardResponse>>(JsonOptions);

                if (result is { IsSuccess: true })
                    return result.Data;
            }

            logger.LogWarning("Dashboard API vratio neuspešan status: {StatusCode}", response.StatusCode);
            return new DashboardResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Greška pri učitavanju dashboard-a");
            throw;
        }
    }
}
