using TicketSystem.UI.Comm;
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
        string language)
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
            content.Add(new StringContent(language), "language");

            HttpResponseMessage response = await _httpClient.PostAsync("api/documents/ask", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<DocumentAudioResponseDto>>(JsonOptions);

                return result ?? new ApiResultWrapper<DocumentAudioResponseDto>
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Server je vratio uspešan status, ali prazan odgovor." }
                };
            }

            // Ako server vrati grešku (npr. 400 Bad Request, 500 Internal Server Error...)
            // Pokušavamo da pročitamo greške ako ih je backend poslao u našem formatu
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResultWrapper<DocumentAudioResponseDto>>(JsonOptions);

                if (errorResult?.Errors != null && errorResult.Errors.Count > 0)
                {
                    return errorResult;
                }
            }
            catch
            {
                // Ako backend nije vratio JSON nego običan tekst, samo nastavljamo dole na fallback poruku
            }

            return new ApiResultWrapper<DocumentAudioResponseDto>
            {
                IsSuccess = false,
                Errors = new List<string> { $"Greška u komunikaciji sa backendom: {(int)response.StatusCode} {response.ReasonPhrase}" }
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
            ApiResultWrapper<UserProfileModel>? result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<UserProfileModel>>(JsonOptions);

            if (result is { IsSuccess: true })
            {
                return result.Data;
            }

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

        ApiResultWrapper<ListResponse<UserSelectModel>>? result =
            await response.Content.ReadFromJsonAsync<
                ApiResultWrapper<ListResponse<UserSelectModel>>>(
                    JsonOptions);

        return result?.IsSuccess == true &&
               result.Data is not null
            ? result.Data.Items
            : [];
    }

    public async Task<List<UserStatsModel>> GetUserStatsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/user/stats");

        if (!response.IsSuccessStatusCode)
            return [];

        ApiResultWrapper<List<UserStatsModel>>? result =
            await response.Content.ReadFromJsonAsync<ApiResultWrapper<List<UserStatsModel>>>(JsonOptions);

        return result?.IsSuccess == true && result.Data is not null ? result.Data : [];
    }

    public async Task<bool> UpdateUserProfileAsync(UserProfileModel model)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/user", model);

        if (response.IsSuccessStatusCode)
        {
            ApiResultWrapper<bool>? result = await response.Content.ReadFromJsonAsync<ApiResultWrapper<bool>>(JsonOptions);

            if (result is not null)
            {
                return result.IsSuccess;
            }

            return true;
        }

        return false;
    }
}
