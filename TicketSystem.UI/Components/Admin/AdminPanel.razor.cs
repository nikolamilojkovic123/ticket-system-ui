using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Resources;

namespace TicketSystem.UI.Components.Admin;

public partial class AdminPanel
{
    [Inject] public IUserService UserService { get; set; } = default!;
    [Inject] public IStringLocalizer<AppResources> L { get; set; } = default!;

    private List<UserStatsModel> stats = [];
    private bool isLoading;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        stats = await UserService.GetUserStatsAsync();
        isLoading = false;
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
}
