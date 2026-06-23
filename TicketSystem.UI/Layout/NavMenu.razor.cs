using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Layout;

public partial class NavMenu : IDisposable
{
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public CultureService CultureService { get; set; } = default!;

    [Inject]
    public IStringLocalizer<AppResources> L { get; set; } = default!;

    [Inject]
    public IConfiguration Configuration { get; set; } = default!;

    [Inject]
    public ILogger<NavMenu> Logger { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    public NotificationService NotificationService { get; set; } = default!;

    private bool isDropdownOpen = false;
    private bool isCollapsed = false;

    private void ToggleSidebar()
    {
        isCollapsed = !isCollapsed;
    }

    protected override void OnInitialized()
    {
        CultureService.OnChange += HandleStateChanged;
        NotificationService.OnChange += HandleStateChanged;
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        CultureService.OnChange -= HandleStateChanged;
        NotificationService.OnChange -= HandleStateChanged;
    }

    private void ToggleDropdown()
    {
        isDropdownOpen = !isDropdownOpen;
    }

    private void LoginGoogle()
    {
        string? googleLoginUrl = Configuration["ApiSettings:GoogleAuthUrl"];

        if (string.IsNullOrWhiteSpace(googleLoginUrl))
        {
            ToastService.ShowToast(
                L["Error"],
                L["GoogleLoginPathNotDefined"],
                ToastType.Error);
            return;
        }

        Nav.NavigateTo(googleLoginUrl, forceLoad: true);
    }

    private async Task ChangeCulture(string culture)
    {
        isDropdownOpen = false;
        await CultureService.SetCultureAsync(culture);
    }

    private async Task Logout()
    {
        try
        {
            if (AuthStateProvider is CustomAuthStateProvider customProvider)
            {
                await customProvider.MarkUserAsLoggedOut();

                Nav.NavigateTo("/", forceLoad: true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, L["LogoutErrorLog"]);

            ToastService.ShowToast(
                L["LogoutError"],
                $"{L["LogoutErrorMessage"]}: {ex.Message}",
                ToastType.Error);
        }
    }
}