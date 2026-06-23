using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources.Index;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Layout;

public partial class Index
{
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IStringLocalizer<IndexResource> L { get; set; } = default!;

    [Inject]
    public IDashboardService dashboardService { get; set; } = default!;

    [Inject]
    public IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    private DashboardResponse dashboard = new();
    private bool isLoading = true;

    private string SeverityBadgeClass => dashboard.AverageSeverity switch
    {
        > 70 => "bg-danger",
        > 40 => "bg-warning text-dark",
        _    => "bg-success"
    };

    private string SeverityBarClass => dashboard.AverageSeverity switch
    {
        > 70 => "bg-danger",
        > 40 => "bg-warning",
        _    => "bg-success"
    };

    private string SeverityLabel => dashboard.AverageSeverity switch
    {
        > 70 => L["Critical"],
        > 40 => L["Medium"],
        _    => L["Stable"]
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            dashboard = await dashboardService.GetDashboardAsync();
        }
        catch (Exception ex)
        {
            ToastService.ShowToast(
                L["Error"],
                L["DashboardLoadingError"],
                ToastType.Error);

            Console.WriteLine(
                $"{L["DashboardLoadingError"]}: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void LoginWithGoogle()
    {
        try
        {
            string? authUrl =
                Configuration["ApiSettings:GoogleAuthUrl"];

            if (string.IsNullOrWhiteSpace(authUrl))
            {
                throw new InvalidOperationException(
                    L["GoogleAuthUrlNotConfigured"]);
            }

            Nav.NavigateTo(authUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            ToastService.ShowToast(
                L["Error"],
                L["GoogleLoginPathNotDefined"],
                ToastType.Error);

            Console.WriteLine($"{L["Error"]}: {ex.Message}");
        }
    }
}