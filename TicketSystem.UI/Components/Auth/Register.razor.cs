using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Auth;

public partial class Register
{
    [Inject] public IAuthService AuthService { get; set; } = default!;
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private readonly RegisterModel model = new();
    private bool isSubmitting;

    private async Task HandleSubmit()
    {
        if (model.Password != model.ConfirmPassword)
        {
            ToastService.ShowToast(L["Error"], L["PasswordsDoNotMatch"], ToastType.Error);
            return;
        }

        isSubmitting = true;

        try
        {
            (bool success, string? errorMessage) = await AuthService.RegisterAsync(
                model.Email, model.Password, model.FirstName, model.LastName);

            if (success)
            {
                Nav.NavigateTo("/", forceLoad: true);
            }
            else
            {
                ToastService.ShowToast(L["Error"], errorMessage ?? L["RegisterFailed"], ToastType.Error);
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
