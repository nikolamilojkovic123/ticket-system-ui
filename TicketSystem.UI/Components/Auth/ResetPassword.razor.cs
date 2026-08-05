using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Auth;

public partial class ResetPassword
{
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IAuthService AuthService { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private readonly ResetPasswordModel model = new();
    private bool isSubmitting;
    private bool resetSuccessful;
    private string? token;

    protected override void OnInitialized()
    {
        Uri uri = Nav.ToAbsoluteUri(Nav.Uri);

        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var tokenValue))
        {
            token = tokenValue.ToString();
        }
    }

    private async Task HandleSubmit()
    {
        if (string.IsNullOrEmpty(token))
            return;

        if (model.NewPassword != model.ConfirmPassword)
        {
            ToastService.ShowToast(L["Error"], L["PasswordsDoNotMatch"], ToastType.Error);
            return;
        }

        isSubmitting = true;

        try
        {
            (bool success, string? errorMessage) = await AuthService.ResetPasswordAsync(token, model.NewPassword);

            if (success)
            {
                resetSuccessful = true;
            }
            else
            {
                ToastService.ShowToast(L["Error"], errorMessage ?? L["ServerErrorOccurred"], ToastType.Error);
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
