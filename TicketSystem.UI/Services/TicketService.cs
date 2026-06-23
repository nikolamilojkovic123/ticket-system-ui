using Microsoft.AspNetCore.WebUtilities;
using TicketSystem.UI.Comm;
using TicketSystem.UI.Enums;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace TicketSystem.UI.Services;

public sealed class TicketService(IHttpClientFactory factory) : ITicketService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient = factory.CreateClient("BackendAPI");

    public async Task<AiResponseDto?> AddMessageAsync(Guid? conversationId, string message)
    {
        var request = new
        {
            ConversationId = conversationId,
            prompt = message
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/ai/ask", request);

        response.EnsureSuccessStatusCode();

        BackendRootResponse<AiResponseDto>? wrapper = await response.Content.ReadFromJsonAsync<BackendRootResponse<AiResponseDto>>(JsonOptions);

        return wrapper?.Response;
    }

    public async Task<Guid> CreateTicketAsync(CreateTicketModel ticket)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/ticket", ticket);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        string errorContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Greška na serveru: {response.StatusCode}. Detalji: {errorContent}");
    }

    public async Task<TicketViewModel?> GetTicketByIdAsync(Guid id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/ticket/{id}");

        if (response.IsSuccessStatusCode)
        {
            ApiResultWrapper<TicketViewModel>? result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<TicketViewModel>>(JsonOptions);

            if (result is { IsSuccess: true })
            {
                return result.Data;
            }

        }

        return null;
    }

    public async Task<bool> UpdateTicketAsync(Guid ticketId, CreateTicketModel ticket)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/ticket/{ticketId}", ticket);

        return response.IsSuccessStatusCode;
    }

    public async Task<PagedResult<TicketViewModel>> GetTicketsAsync(int page, int pageSize, TicketFilterModel? filter = null)
    {
        List<string> parts = new()
        { $"page={page}", $"pageSize={pageSize}" };

        if (filter is not null)
        {
            foreach (TicketStatus s in filter.Statuses) parts.Add($"statuses={Uri.EscapeDataString(s.ToString())}");
            foreach (TicketPriority p in filter.Priorities) parts.Add($"priorities={Uri.EscapeDataString(p.ToString())}");
            foreach (TicketCategory c in filter.Categories) parts.Add($"categories={Uri.EscapeDataString(c.ToString())}");
            if (filter.AssigneeId.HasValue) parts.Add($"assigneeId={filter.AssigneeId}");
            if (filter.DateFrom.HasValue) parts.Add($"dateFrom={filter.DateFrom.Value:yyyy-MM-dd}");
            if (filter.DateTo.HasValue) parts.Add($"dateTo={filter.DateTo.Value:yyyy-MM-dd}");
        }

        string url = $"api/ticket?{string.Join("&", parts)}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PagedResult<TicketViewModel>>()
                   ?? new PagedResult<TicketViewModel>();

        throw new HttpRequestException($"Greška pri dohvatanju tiketa: {response.ReasonPhrase}");
    }

    public async Task<PagedResult<TicketViewModel>> SearchTicketsAsync(
        string query,
        int page,
        int pageSize)
    {
        Dictionary<string, string?> queryParams = new()
            {
                { "query", query },
                { "page", page.ToString() },
                { "pageSize", pageSize.ToString() }
            };

        string url = QueryHelpers.AddQueryString("api/ticket/search", queryParams);

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        ApiResponse<PagedResult<TicketViewModel>>? wrapper =
            await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TicketViewModel>>>(JsonOptions);

        return wrapper?.Data
               ?? new PagedResult<TicketViewModel>();
    }

    public async Task<ApiResultWrapper<TicketCommentDto>> AddTicketCommentAsync(Guid ticketId, string content)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/ticket/{ticketId}/comments", new { Content = content });

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResultWrapper<TicketCommentDto>
            {
                IsSuccess = false,
                Errors = [$"HTTP {(int)response.StatusCode}"]
            };
        }

        TicketCommentDto? result = await response.Content.ReadFromJsonAsync<TicketCommentDto>(JsonOptions);

        return new ApiResultWrapper<TicketCommentDto>
        {
            IsSuccess = true,
            Data = result ?? new TicketCommentDto()
        };
    }

    public async Task<List<TicketCommentDto>> GetTicketCommentsAsync(Guid ticketId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/ticket/{ticketId}/comments");

        ApiResultWrapper<List<TicketCommentDto>>? result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<List<TicketCommentDto>>>(JsonOptions);

        return result?.Data ?? new List<TicketCommentDto>();
    }
}
