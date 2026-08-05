using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Auth;

public partial class Login
{
    [Inject] public IAuthService AuthService { get; set; } = default!;
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private readonly LoginModel model = new();
    private bool isSubmitting;

    private async Task HandleSubmit()
    {
        isSubmitting = true;

        try
        {
            (bool success, string? errorMessage) = await AuthService.LoginAsync(model.Email, model.Password);

            if (success)
            {
                Nav.NavigateTo("/", forceLoad: true);
            }
            else
            {
                ToastService.ShowToast(L["Error"], errorMessage ?? L["LoginFailed"], ToastType.Error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowToast(L["Error"], L["ServerErrorOccurred"], ToastType.Error);
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
