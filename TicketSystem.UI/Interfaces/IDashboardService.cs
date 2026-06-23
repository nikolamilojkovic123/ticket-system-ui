using TicketSystem.UI.Models;

namespace TicketSystem.UI.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync();
}
