using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Comm;
using TicketSystem.UI.Enums;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources.Ticket;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Tickets;

public partial class TicketList
{
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public ITicketService TicketService { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;
    [Inject] public IStringLocalizer<TicketResource> L { get; set; } = default!;
    [Inject] public ToastService ToastService { get; set; } = default!;
    private PagedResult<TicketViewModel>? pagedResult;
    private List<TicketViewModel>? tickets;
    private int currentPage = 1;
    private int pageSize = 5;
    private string searchQuery = "";
    private bool isSearching;
    private bool isLoading;

    private int TotalPages => pagedResult == null || pagedResult.TotalCount == 0
        ? 0 : (int)Math.Ceiling((double)pagedResult.TotalCount / pageSize);
    private bool HasNext => currentPage < TotalPages;
    private bool HasPrev => currentPage > 1;
    private bool _isKanbanView;
    private List<TicketViewModel> _allTickets = new();
    private TicketViewModel? _dragging;
    private TicketStatus? _dragOverColumn;

    private record KanbanColumn(TicketStatus Status, string Label, string DotClass);
    private readonly KanbanColumn[] _columns =
    [
        new(TicketStatus.Open,               "Open",        "dot-open"),
        new(TicketStatus.InProgress,         "In Progress", "dot-inprogress"),
        new(TicketStatus.WaitingforCustomer, "Waiting",     "dot-waiting"),
        new(TicketStatus.Closed,             "Closed",      "dot-closed"),
    ];

    private TicketFilterModel _filter = new();
    private bool _showFilters;
    private ICollection<UserSelectModel> _users = [];

    private List<TicketViewModel> KanbanFilteredTickets => ApplyInMemoryFilter(_allTickets);
    private List<TicketViewModel>? TableFilteredTickets => tickets is null ? null : ApplyInMemoryFilter(tickets);

    private List<TicketViewModel> ApplyInMemoryFilter(List<TicketViewModel> source)
    {
        IEnumerable<TicketViewModel> q = source;
        if (_filter.Statuses.Count > 0) q = q.Where(t => _filter.Statuses.Contains(t.Status));
        if (_filter.Priorities.Count > 0) q = q.Where(t => _filter.Priorities.Contains(t.Priority));
        if (_filter.Categories.Count > 0) q = q.Where(t => _filter.Categories.Contains(t.Category));
        if (_filter.AssigneeId.HasValue) q = q.Where(t => t.UserId == _filter.AssigneeId);
        return q.ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        _users = await UserService.GetUsersAsync();
        await LoadTickets();
    }

    private async Task SwitchToKanban() { _isKanbanView = true; await LoadAllTickets(); }
    private void SwitchToTable() { _isKanbanView = false; _dragging = null; _dragOverColumn = null; }

    private async Task LoadTickets()
    {
        try
        {
            isLoading = true;
            pagedResult = await TicketService.GetTicketsAsync(currentPage, pageSize, _filter.IsEmpty ? null : _filter);
            tickets = pagedResult?.Items ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ToastService.ShowToast(L["Error"], L["ServerProblemTryAgain"], ToastType.Error);
            pagedResult = new() { Items = new(), TotalCount = 0 };
            tickets = new();
        }
        finally { isLoading = false; }
    }

    private async Task OnPageSizeChanged() { currentPage = 1; await LoadTickets(); }
    private async Task NextPage() { if (HasNext) { currentPage++; await LoadTickets(); } }
    private async Task PrevPage() { if (HasPrev) { currentPage--; await LoadTickets(); } }

    private async Task ClearSearch()
    {
        currentPage = 1; searchQuery = ""; isSearching = false;
        await LoadTickets();
    }

    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(searchQuery)) return;
        isLoading = true; isSearching = true; StateHasChanged();
        try
        {
            pagedResult = await TicketService.SearchTicketsAsync(searchQuery, currentPage, pageSize);
            tickets = pagedResult?.Items ?? new();
        }
        catch { tickets = new(); ToastService.ShowToast(L["SearchError"], L["TicketSearchError"], ToastType.Error); }
        finally { isLoading = false; StateHasChanged(); }
    }

    private async Task HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await PerformSearch();
    }

    // ── Kanban data ──────────────────────────────────────
    private async Task LoadAllTickets()
    {
        try
        {
            isLoading = true;
            PagedResult<TicketViewModel> result = await TicketService.GetTicketsAsync(1, 500);
            _allTickets = result?.Items ?? new();
        }
        catch { _allTickets = new(); ToastService.ShowToast(L["Error"], L["ServerProblemTryAgain"], ToastType.Error); }
        finally { isLoading = false; }
    }

    // ── Drag & Drop ──────────────────────────────────────
    private void OnDragStart(TicketViewModel ticket) => _dragging = ticket;
    private void OnDragEnd() { _dragging = null; _dragOverColumn = null; }

    private async Task OnDrop(TicketStatus targetStatus)
    {
        if (_dragging is null || _dragging.Status == targetStatus) { OnDragEnd(); return; }
        TicketViewModel ticket = _dragging;
        TicketStatus prev = ticket.Status;
        ticket.Status = targetStatus;
        _dragging = null; _dragOverColumn = null;
        StateHasChanged();
        try
        {
            bool ok = await TicketService.UpdateTicketAsync(ticket.Id, new CreateTicketModel
            {
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = targetStatus,
                UserId = ticket.UserId
            });
            if (!ok) { ticket.Status = prev; ToastService.ShowToast(L["Error"], L["ServerProblemTryAgain"], ToastType.Error); StateHasChanged(); }
        }
        catch { ticket.Status = prev; StateHasChanged(); }
    }

    // ── Filter logic ─────────────────────────────────────
    private async Task ToggleFilter<T>(HashSet<T> set, T value)
    {
        if (!set.Remove(value)) set.Add(value);
        await ApplyFilters();
    }

    private async Task ApplyFilters()
    {
        currentPage = 1;
        if (_isKanbanView) StateHasChanged();
        else await LoadTickets();
    }

    private void OnAssigneeChanged(ChangeEventArgs e)
    {
        _filter.AssigneeId = Guid.TryParse(e.Value?.ToString(), out Guid id) ? id : null;
        _ = ApplyFilters();
    }

    private void OnDateChanged(ChangeEventArgs e, bool isFrom)
    {
        DateTime? val = DateTime.TryParse(e.Value?.ToString(), out DateTime d) ? d : (DateTime?)null;
        if (isFrom) _filter.DateFrom = val;
        else _filter.DateTo = val;
        _ = ApplyFilters();
    }

    private async Task ClearFilters()
    {
        _filter = new TicketFilterModel();
        await ApplyFilters();
    }

    // ── Navigation ───────────────────────────────────────
    private void GoToCreate() => Nav.NavigateTo("/ticket/create");
    private void OpenDetails(Guid id) => Nav.NavigateTo($"/ticket/edit/{id}");

    // ── UI helpers ───────────────────────────────────────
    private string GetPriorityClass(TicketPriority p) => p switch
    {
        TicketPriority.High => "bg-danger-subtle text-danger",
        TicketPriority.Medium => "bg-warning-subtle text-warning-emphasis",
        TicketPriority.Low => "bg-info-subtle text-info",
        _ => "bg-secondary-subtle text-secondary"
    };

    private string GetStatusClass(TicketStatus s) => s switch
    {
        TicketStatus.Open => "bg-success",
        TicketStatus.InProgress => "bg-primary",
        TicketStatus.WaitingforCustomer => "bg-purple",
        TicketStatus.Closed => "bg-secondary",
        _ => "bg-dark"
    };

    private string GetCategoryIcon(TicketCategory c) => c switch
    {
        TicketCategory.Hardware => "bi-cpu",
        TicketCategory.Software => "bi-code-slash",
        TicketCategory.Network => "bi-wifi",
        TicketCategory.Email => "bi-envelope",
        TicketCategory.Other => "bi-patch-question",
        _ => "bi-tag"
    };

    // Filter chip helpers
    private string GetStatusLabel(TicketStatus s) => s switch
    {
        TicketStatus.Open => "Open",
        TicketStatus.InProgress => "In Progress",
        TicketStatus.WaitingforCustomer => "Waiting",
        TicketStatus.Closed => "Closed",
        _ => s.ToString()
    };

    private string GetStatusChipClass(TicketStatus s) => s switch
    {
        TicketStatus.Open => "chip-open",
        TicketStatus.InProgress => "chip-inprogress",
        TicketStatus.WaitingforCustomer => "chip-waiting",
        TicketStatus.Closed => "chip-closed",
        _ => ""
    };

    private string GetStatusDotClass(TicketStatus s) => s switch
    {
        TicketStatus.Open => "dot-open",
        TicketStatus.InProgress => "dot-inprogress",
        TicketStatus.WaitingforCustomer => "dot-waiting",
        TicketStatus.Closed => "dot-closed",
        _ => ""
    };

    private string GetPriorityChipClass(TicketPriority p) => p switch
    {
        TicketPriority.High => "chip-high",
        TicketPriority.Medium => "chip-medium",
        TicketPriority.Low => "chip-low",
        _ => ""
    };

    private string GetPriorityIcon(TicketPriority p) => p switch
    {
        TicketPriority.High => "🔴",
        TicketPriority.Medium => "🟡",
        TicketPriority.Low => "🟢",
        _ => ""
    };
}
