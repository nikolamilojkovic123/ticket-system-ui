using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources.User;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.UserProfile;

public partial class UserProfile
{
    [Inject]
    public NavigationManager? Navigation { get; set; } = default!;
    [Inject]
    public IUserService? userService { get; set; } = default!;
    [Inject]
    private ToastService ToastService { get; set; } = default!;
    [Inject]
    public IStringLocalizer<UserResource> Localizer { get; set; } = default!;
    private UserProfileModel model = new();
    private bool isLoading = true;
    private bool isSaving = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserProfile();
    }
    private async Task LoadUserProfile()
    {
        try
        {
            UserProfileModel? result = await userService!.GetUserProfileInfoAsync();
            if (result != null)
            {
                model = result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Greška pri učitavanju profila: " + ex.Message);
        }
        finally
        {
            isLoading = false;
        }
    }
    private async Task HandleSubmit()
    {
        isSaving = true;

        try
        {
            bool response = await userService!.UpdateUserProfileAsync(model);

            if (response)
            {
                ToastService.ShowToast(
                    Localizer["Updated"],
                    Localizer["ProfileUpdatedSuccessfully"],
                    ToastType.Success);
            }
            else
            {
                ToastService.ShowToast(
                    Localizer["Error"],
                    Localizer["ErrorWhileSavingProfile"],
                    ToastType.Error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowToast(
                Localizer["Error"],
                Localizer["ServerErrorOccurred"],
                ToastType.Error);
        }
        finally
        {
            isSaving = false;
        }
    }
    public async Task ResetForm()
    {
        await LoadUserProfile();
    }
}
