using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using TicketSystem.UI.Comm;
using TicketSystem.UI.Interfaces;
using TicketSystem.UI.Models;
using TicketSystem.UI.Models.Toast;
using TicketSystem.UI.Resources.DocumentAi;
using TicketSystem.UI.Services;

namespace TicketSystem.UI.Components.Shared.AiAssistant;

public partial class DocumentAiChat
{
    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private ITicketService TicketService { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private IStringLocalizer<DocumentAiResource> L { get; set; } = default!;

    private DocumentAskModel _askModel = new();
    private IBrowserFile? _selectedFile;
    private bool _isProcessing;
    private string _loadingStage = string.Empty;
    private string _aiResponse = string.Empty;
    private string _errorMessage = string.Empty;
    private string _audioSource = string.Empty;

    private string _selectedLanguage = "sr";

    private List<TicketViewModel> _tickets = new();
    private string _selectedTicketIdRaw = string.Empty;
    private bool _isSendingToTicket;

    private bool CanSubmit => _selectedFile != null && !string.IsNullOrWhiteSpace(_askModel.Question) && !_isProcessing;

    private bool CanSendToTicket =>
        !_isSendingToTicket
        && !string.IsNullOrEmpty(_aiResponse)
        && Guid.TryParse(_selectedTicketIdRaw, out _);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            PagedResult<TicketViewModel> result = await TicketService.GetTicketsAsync(1, 100);
            _tickets = result.Items;
        }
        catch (Exception)
        {
            _tickets = new();
        }
    }

    private async Task SendAnswerToTicketAsync()
    {
        if (!Guid.TryParse(_selectedTicketIdRaw, out Guid ticketId))
        {
            return;
        }

        _isSendingToTicket = true;

        try
        {
            string content = $"{L["AiAnswerCommentPrefix"]}\n\n{_aiResponse}";

            ApiResultWrapper<TicketCommentDto> result = await TicketService.AddTicketCommentAsync(ticketId, content);

            if (result.IsSuccess)
            {
                ToastService.ShowToast(
                    title: L["AiAnswerTitle"],
                    message: L["CommentAddedSuccess"],
                    type: ToastType.Success);
            }
            else
            {
                ToastService.ShowToast(
                    title: L["AiAnswerTitle"],
                    message: L["CommentAddedError"],
                    type: ToastType.Error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowToast(
                title: L["AiAnswerTitle"],
                message: L["CommentAddedError"],
                type: ToastType.Error);
        }
        finally
        {
            _isSendingToTicket = false;
            StateHasChanged();
        }
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
        _askModel.FileName = e.File.Name;
        _errorMessage = string.Empty;
        _aiResponse = string.Empty;
        _audioSource = string.Empty;
    }

    private async Task HandleSubmitAsync()
    {
        if (_selectedFile == null) return;

        _isProcessing = true;
        _aiResponse = string.Empty;
        _errorMessage = string.Empty;
        _audioSource = string.Empty;

        try
        {
            _loadingStage = L["LoadingStageUpload"];
            StateHasChanged();

            Stream fileStream = _selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

            _loadingStage = L["LoadingStageAnalyze"];
            StateHasChanged();

            ApiResultWrapper<DocumentAudioResponseDto> result = await UserService.AskDocumentAsync(
                fileStream,
                _selectedFile.Name,
                _selectedFile.ContentType,
                _askModel.Question,
                _selectedLanguage);

            if (result.IsSuccess && result.Data is not null)
            {
                _aiResponse = result.Data.Text ?? L["EmptyResponse"];

                if (!string.IsNullOrEmpty(result.Data.AudioBase64))
                {
                    _audioSource = $"data:audio/mp3;base64,{result.Data.AudioBase64}";
                }
            }
            else if (result.Errors is not null && result.Errors.Count > 0)
            {
                _errorMessage = string.Join(", ", result.Errors);
            }
            else
            {
                _errorMessage = L["UnknownError"];
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"{L["ClientErrorPrefix"]} {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
            _loadingStage = string.Empty;
            StateHasChanged();
        }
    }
}