using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Settings;

public partial class SettingsPage
{
    [Inject]
    public ThemeService ThemeService { get; set; } = default!;

    [Inject]
    public CultureService CultureService { get; set; } = default!;

    [Inject]
    public ISyncLocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    public IStringLocalizer<AppResources> L { get; set; } = default!;

    private string CurrentTheme => ThemeService.CurrentTheme;

    private string CurrentCulture => LocalStorage.GetItem<string>("Language") ?? "sr";

    private async Task SetTheme(string theme)
    {
        await ThemeService.SetThemeAsync(theme);
        StateHasChanged();
    }

    private async Task ChangeCulture(string culture)
    {
        await CultureService.SetCultureAsync(culture);
        StateHasChanged();
    }
}
