using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Enums;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Admin;

public partial class AdminPanel
{
    [Inject] public IUserService UserService { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<UserStatsModel> stats = [];
    private List<AdminUserModel> adminUsers = [];
    private readonly HashSet<Guid> savingRoleUserIds = [];
    private bool isLoading;
    private bool isLoadingUsers;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        isLoadingUsers = true;

        stats = await UserService.GetUserStatsAsync();
        isLoading = false;

        adminUsers = await UserService.GetUsersForAdminAsync();
        isLoadingUsers = false;
    }

    private int TotalTickets => stats.Sum(s => s.Total);
    private int TotalClosed => stats.Sum(s => s.ClosedCount);
    private int OverallResolutionRate => TotalTickets > 0 ? (int)Math.Round((double)TotalClosed / TotalTickets * 100) : 0;
    private string TopPerformer => stats.FirstOrDefault()?.Name ?? "—";

    private static string ResolutionBarClass(double rate) => rate switch
    {
        >= 75 => "bg-success",
        >= 40 => "bg-warning",
        _     => "bg-danger",
    };

    private async Task OnRoleChanged(AdminUserModel user, ChangeEventArgs e)
    {
        if (!Enum.TryParse(e.Value?.ToString(), out UserRole newRole) || newRole == user.Role)
            return;

        UserRole previousRole = user.Role;
        user.Role = newRole;
        savingRoleUserIds.Add(user.Id);

        try
        {
            bool success = await UserService.UpdateUserRoleAsync(user.Id, newRole);

            if (success)
            {
                ToastService.ShowToast(L["Updated"], L["RoleUpdatedSuccessfully"], ToastType.Success);
            }
            else
            {
                user.Role = previousRole;
                ToastService.ShowToast(L["Error"], L["ErrorWhileSavingRole"], ToastType.Error);
            }
        }
        catch (Exception)
        {
            user.Role = previousRole;
            ToastService.ShowToast(L["Error"], L["ServerErrorOccurred"], ToastType.Error);
        }
        finally
        {
            savingRoleUserIds.Remove(user.Id);
        }
    }
}
