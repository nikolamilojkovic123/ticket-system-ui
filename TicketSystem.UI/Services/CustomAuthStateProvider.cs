using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = await _storage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            if (IsTokenExpired(token))
            {
                await _storage.RemoveItemAsync("authToken");
                return new AuthenticationState(_anonymous);
            }

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    private bool IsTokenExpired(string jwt)
    {
        try
        {
            string[] parts = jwt.Split('.');
            if (parts.Length != 3)
                return true;

            byte[] jsonBytes = ParseBase64WithoutPadding(parts[1]);
            var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (claims != null && claims.TryGetValue("exp", out object? expValue)
                && long.TryParse(expValue.ToString(), out long exp))
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);

        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task SignInAsync(string token)
    {
        await _storage.SetItemAsync("authToken", token);
        MarkUserAsAuthenticated(token);
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _storage.RemoveItemAsync("authToken"); // Brišemo token iz memorije
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    // KLJUČNA METODA: Izvlači Email, Name i Role direktno iz JWT stringa
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        string[] parts = jwt.Split('.');
        if (parts.Length != 3)
            return claims;

        byte[] jsonBytes = ParseBase64WithoutPadding(parts[1]);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                // Mapiranje standardnih JWT ključeva na .NET ClaimTypes
                string type = kvp.Key switch
                {
                    "email" => ClaimTypes.Email,
                    "unique_name" => ClaimTypes.Name,
                    "name" => ClaimTypes.Name,
                    "role" => ClaimTypes.Role,
                    _ => kvp.Key
                };

                claims.Add(new Claim(type, kvp.Value.ToString() ?? ""));
            }
        }
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}