using Microsoft.AspNetCore.Components;

namespace TicketSystem.UI.Services
{
    public class AuthorizationHandler(Blazored.LocalStorage.ILocalStorageService localStorage, NavigationManager nav) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string? token = await localStorage.GetItemAsync<string>("authToken", ct);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await base.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await localStorage.RemoveItemAsync("authToken");

                nav.NavigateTo("/login-error");

            }

            return response;
        }
    }
}
