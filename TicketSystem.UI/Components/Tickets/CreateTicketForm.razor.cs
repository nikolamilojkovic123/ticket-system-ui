using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Enums;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources.Ticket;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Tickets;

public partial class CreateTicketForm
{
    [Parameter]
    public Guid? Id { get; set; }

    [Inject]
    private ITicketService TicketService { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    public IStringLocalizer<TicketResource> L { get; set; } = default!;

    [Inject]
    private IUserService _userService { get; set; } = default!;
    public CreateTicketModel Ticket { get; set; } = new();
    public bool IsLoading { get; set; }
    public string? Error { get; set; }
    public string? SelectedFileName { get; set; }
    public bool IsAiOpen { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();

    private Guid? _activeConversationId;
    private List<string> _categories = new();
    private List<string> _priorities = new();
    private List<string> _statuses = new();
    private ICollection<UserSelectModel> _users = [];
    private Guid? SelectedUserId;
    private bool IsEditMode => Id.HasValue && Id.Value != Guid.Empty;
    private List<TicketCommentDto> _comments = new();
    private string _newComment = string.Empty;
    private bool _isSubmittingComment;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _categories = GetEnumNames<TicketCategory>();
            _priorities = GetEnumNames<TicketPriority>();
            _statuses = GetEnumNames<TicketStatus>();
            _users = await _userService.GetUsersAsync();

            if (IsEditMode)
            {
                await LoadTicketData();
            }
        }
        catch (Exception ex)
        {
            Error = $"{L["InitializationError"]}: {ex.Message}";

            ToastService.ShowToast(
                L["Error"],
                L["ErrorLoadingForm"],
                ToastType.Error);
        }
    }

    private async Task LoadTicketData()
    {
        try
        {
            IsLoading = true;

            TicketViewModel? existingTicket =
                await TicketService.GetTicketByIdAsync(Id!.Value);

            if (existingTicket != null)
            {
                Ticket = new CreateTicketModel
                {
                    Title = existingTicket.Title,
                    Description = existingTicket.Description,
                    Category = existingTicket.Category,
                    Priority = existingTicket.Priority,
                    UserId = existingTicket.UserId
                };
                SelectedUserId = existingTicket.UserId;
            }

            _comments =
                await TicketService.GetTicketCommentsAsync(Id!.Value);
        }
        catch (Exception ex)
        {
            ToastService.ShowToast(
                L["Error"],
                L["ErrorLoadingTicketData"],
                ToastType.Error);

            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            IsLoading = true;
            Error = null;

            Ticket.UserId = SelectedUserId;

            if (IsEditMode)
            {
                bool success =
                    await TicketService.UpdateTicketAsync(
                        Id!.Value,
                        Ticket);

                if (success)
                {
                    ToastService.ShowToast(
                        L["Updated"],
                        L["TicketUpdatedSuccessfully"],
                        ToastType.Success);

                    await Task.Delay(1000);

                    NavigationManager.NavigateTo("/tickets");
                }
            }
            else
            {
                Guid ticketId =
                    await TicketService.CreateTicketAsync(Ticket);

                if (ticketId != Guid.Empty)
                {
                    ToastService.ShowToast(
                        L["Success"],
                        L["TicketCreatedSuccessfully"],
                        ToastType.Success);

                    await Task.Delay(1000);

                    NavigationManager.NavigateTo("/tickets");
                }
            }
        }
        catch (Exception ex)
        {
            Error = $"{L["RequestSubmissionError"]}: {ex.Message}";

            ToastService.ShowToast(
                L["ServerError"],
                L["ErrorSubmittingRequest"],
                ToastType.Error);
        }
        finally
        {
            IsLoading = false;

            StateHasChanged();
        }
    }

    private void Cancel()
    {
        Ticket = new CreateTicketModel();
        SelectedFileName = null;
        Error = null;
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        IBrowserFile file = e.File;
        SelectedFileName = file.Name;
    }

    private void ToggleAi()
    {
        IsAiOpen = !IsAiOpen;
    }

    private async Task SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Messages.Add(new ChatMessage
        {
            Text = message,
            IsUser = true
        });

        string currentPrompt = message;
        message = string.Empty;

        try
        {
            AiResponseDto? response =
                await TicketService.AddMessageAsync(
                    _activeConversationId,
                    currentPrompt);

            if (response == null)
            {
                return;
            }

            _activeConversationId = response.ConversationId;

            if (response.Type == "message")
            {
                Messages.Add(new ChatMessage
                {
                    Text = response.Content ?? string.Empty,
                    IsUser = false
                });
            }

            if (response.Type == "action" && response.Data != null)
            {
                Ticket.Title =
                    response.Data.Title ?? Ticket.Title;

                Ticket.Description =
                    response.Data.Description ?? Ticket.Description;

                Ticket.Category =
                    Enum.TryParse<TicketCategory>(
                        response.Data.Category,
                        out TicketCategory cat)
                        ? cat
                        : Ticket.Category;

                Ticket.Priority =
                    Enum.TryParse<TicketPriority>(
                        response.Data.Priority,
                        out TicketPriority prio)
                        ? prio
                        : Ticket.Priority;

                Messages.Add(new ChatMessage
                {
                    Text = L["AiFieldsAutoFilled"],
                    IsUser = false
                });
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowToast(
                L["Error"],
                L["AiCommunicationError"],
                ToastType.Error);

            Console.WriteLine(
                $"Greška pri komunikaciji sa AI servisom: {ex.Message}");
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task AddComment()
    {
        if (string.IsNullOrWhiteSpace(_newComment) || !IsEditMode) return;

        _isSubmittingComment = true;
        try
        {
            ApiResultWrapper<TicketCommentDto> result = await TicketService.AddTicketCommentAsync(Id!.Value, _newComment.Trim());
            if (result.IsSuccess && result.Data is not null)
            {
                _comments.Add(result.Data);
                _newComment = string.Empty;
            }
            else
            {
                ToastService.ShowToast(L["Error"], L["ErrorSubmittingRequest"], ToastType.Error);
            }
        }
        catch
        {
            ToastService.ShowToast(L["Error"], L["ErrorSubmittingRequest"], ToastType.Error);
        }
        finally
        {
            _isSubmittingComment = false;
            StateHasChanged();
        }
    }

    private async Task HandleCommentKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && e.CtrlKey)
            await AddComment();
    }

    private static string FormatCommentTime(DateTime dt)
    {
        TimeSpan diff = DateTime.UtcNow - dt.ToUniversalTime();
        if (diff.TotalMinutes < 1) return "Upravo";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d";
        return dt.ToLocalTime().ToString("dd MMM");
    }

    private List<string> GetEnumNames<T>() where T : Enum
        => Enum.GetNames(typeof(T)).ToList();
}