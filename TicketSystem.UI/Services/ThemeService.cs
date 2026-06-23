using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace TicketSystem.UI.Services;

public class ThemeService
{
    private const string StorageKey = "Theme";

    private readonly ISyncLocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;

    public ThemeService(ISyncLocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public string CurrentTheme => _localStorage.GetItem<string>(StorageKey) ?? "light";

    public async Task SetThemeAsync(string theme)
    {
        _localStorage.SetItem(StorageKey, theme);
        await _jsRuntime.InvokeVoidAsync("themeInterop.setTheme", theme);
    }
}
