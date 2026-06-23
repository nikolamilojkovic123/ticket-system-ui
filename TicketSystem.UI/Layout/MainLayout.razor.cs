using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Layout;

public partial class MainLayout : IDisposable
{
    [Inject]
    public CultureService CultureService { get; set; } = default!;

    [Inject]
    public NotificationService NotificationService { get; set; } = default!;

    [Inject]
    public SignalRService SignalRService { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    private bool isMobileMenuOpen = false;

    protected override void OnInitialized()
    {
        NotificationService.OnChange += HandleStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JS.InvokeVoidAsync("eval", "document.body.classList.add('loaded')");

        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
            await SignalRService.StartAsync();
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    private void ToggleMobileMenu() => isMobileMenuOpen = !isMobileMenuOpen;
    private void CloseMobileMenu() => isMobileMenuOpen = false;

    public void Dispose()
    {
        NotificationService.OnChange -= HandleStateChanged;
    }
}
