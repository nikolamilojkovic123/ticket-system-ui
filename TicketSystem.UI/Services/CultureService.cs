using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace TicketSystem.UI.Services;

public class CultureService
{
    private const string StorageKey = "Language";

    private readonly ISyncLocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;

    public event Action? OnChange;

    public CultureService(ISyncLocalStorageService localStorage, NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
    }

    public string CurrentCulture => _localStorage.GetItem<string>(StorageKey) ?? "sr";

    public async Task SetCultureAsync(string culture)
    {
        if (culture == CurrentCulture)
            return;

        _localStorage.SetItem(StorageKey, culture);

        await _jsRuntime.InvokeVoidAsync("eval", "document.body.style.opacity='0'; document.body.style.transition='opacity 0.15s'");

        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }
}
