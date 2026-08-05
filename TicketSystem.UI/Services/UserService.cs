using TicketSystem.UI.Comm;
using TicketSystem.UI.Enums;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TicketSystem.UI.Services;

public sealed class UserService(IHttpClientFactory factory) : IUserService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient = factory.CreateClient("BackendAPI");
    public async Task<ApiResultWrapper<DocumentAudioResponseDto>> AskDocumentAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string question,
        string voice)
    {
        try
        {
            using MultipartFormDataContent content = new();

            using MemoryStream memoryStream = new();
            await fileStream.CopyToAsync(memoryStream);
            byte[] buffer = memoryStream.ToArray();

            ByteArrayContent byteContent = new(buffer);

            string resolvedContentType = string.IsNullOrEmpty(contentType)
                ? "application/octet-stream"
                : contentType;
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(resolvedContentType);

            content.Add(byteContent, "file", fileName);
            content.Add(new StringContent(question), "question");
            content.Add(new StringContent(voice), "voice");

            HttpResponseMessage response = await _httpClient.PostAsync("api/documents/ask", content);

            if (response.IsSuccessStatusCode)
            {
                DocumentAudioResponseDto? result = await response.Content.ReadFromJsonAsync<DocumentAudioResponseDto>(JsonOptions);

                return result is not null
                    ? new ApiResultWrapper<DocumentAudioResponseDto> { IsSuccess = true, Data = result }
                    : new ApiResultWrapper<DocumentAudioResponseDto>
                    {
                        IsSuccess = false,
                        Errors = new List<string> { "Server je vratio uspešan status, ali prazan odgovor." }
                    };
            }

            return new ApiResultWrapper<DocumentAudioResponseDto>
            {
                IsSuccess = false,
                Errors = await ApiErrorReader.ReadErrorMessagesAsync(response, JsonOptions)
            };
        }
        catch (JsonException jsonEx)
        {
            return new ApiResultWrapper<DocumentAudioResponseDto>
            {
                IsSuccess = false,
                Errors = new List<string> { $"Greška pri parsiranju podataka sa servera: {jsonEx.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new ApiResultWrapper<DocumentAudioResponseDto>
            {
                IsSuccess = false,
                Errors = new List<string> { $"Greška u servisu klijenta: {ex.Message}" }
            };
        }
    }
    public async Task<UserProfileModel?> GetUserProfileInfoAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/user");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfileModel>(JsonOptions);
        }

        return null;
    }

    public async Task<ICollection<UserSelectModel>> GetUsersAsync()
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync("api/user/all");

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        ListResponse<UserSelectModel>? result =
            await response.Content.ReadFromJsonAsync<ListResponse<UserSelectModel>>(JsonOptions);

        return result?.Items ?? [];
    }

    public async Task<List<UserStatsModel>> GetUserStatsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/user/stats");

        if (!response.IsSuccessStatusCode)
            return [];

        List<UserStatsModel>? result =
            await response.Content.ReadFromJsonAsync<List<UserStatsModel>>(JsonOptions);

        return result ?? [];
    }

    public async Task<bool> UpdateUserProfileAsync(UserProfileModel model)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/user", model);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<AdminUserModel>> GetUsersForAdminAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/user/admin/all");

        if (!response.IsSuccessStatusCode)
            return [];

        ListResponse<AdminUserModel>? result =
            await response.Content.ReadFromJsonAsync<ListResponse<AdminUserModel>>(JsonOptions);

        return result is not null ? [.. result.Items] : [];
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole role)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/user/{userId}/role", new { Role = role });

        return response.IsSuccessStatusCode;
    }
}
