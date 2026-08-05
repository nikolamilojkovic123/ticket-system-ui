using Microsoft.AspNetCore.Components.Authorization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace TicketSystem.UI.Services;

public sealed class AuthService(IHttpClientFactory factory, AuthenticationStateProvider authStateProvider) : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient = factory.CreateClient("BackendAPI");
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public Task<(bool Success, string? ErrorMessage)> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var payload = new { Email = email, Password = password, FirstName = firstName, LastName = lastName };
        return PostAndSignInAsync("api/auth/register", payload);
    }

    public Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        var payload = new { Email = email, Password = password };
        return PostAndSignInAsync("api/auth/login", payload);
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var payload = new { Email = email };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", payload);

        if (!response.IsSuccessStatusCode)
            return false;

        AuthApiResponse? result = await response.Content.ReadFromJsonAsync<AuthApiResponse>(JsonOptions);
        return result?.Success == true;
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string token, string newPassword)
    {
        var payload = new { Token = token, NewPassword = newPassword };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", payload);

        if (!response.IsSuccessStatusCode)
            return (false, null);

        AuthApiResponse? result = await response.Content.ReadFromJsonAsync<AuthApiResponse>(JsonOptions);
        return (result?.Success == true, result?.ErrorMessage);
    }

    private async Task<(bool Success, string? ErrorMessage)> PostAndSignInAsync(string url, object payload)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
            return (false, null);

        AuthApiResponse? result = await response.Content.ReadFromJsonAsync<AuthApiResponse>(JsonOptions);

        if (result?.Success != true || result.Data is null)
            return (false, result?.ErrorMessage);

        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            await customProvider.SignInAsync(result.Data.Token);
        }

        return (true, null);
    }
}
