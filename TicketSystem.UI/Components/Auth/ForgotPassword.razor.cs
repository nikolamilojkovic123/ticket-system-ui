using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Auth;

public partial class ForgotPassword
{
    [Inject] public IAuthService AuthService { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private readonly ForgotPasswordModel model = new();
    private bool isSubmitting;
    private bool linkSent;

    private async Task HandleSubmit()
    {
        isSubmitting = true;

        try
        {
            bool success = await AuthService.ForgotPasswordAsync(model.Email);

            if (success)
            {
                linkSent = true;
            }
            else
            {
                ToastService.ShowToast(L["Error"], L["ServerErrorOccurred"], ToastType.Error);
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
