using TicketSystem.UI.Comm;
using TicketSystem.UI.Models;

namespace TicketSystem.UI.Interfaces;

public interface ITicketService
{
    Task<TicketViewModel?> GetTicketByIdAsync(Guid id);
    Task<Guid> CreateTicketAsync(CreateTicketModel ticket);
    Task<bool> UpdateTicketAsync(Guid ticketId, CreateTicketModel ticket);
    Task<AiResponseDto?> AddMessageAsync(Guid? conversationId, string message);
    Task<PagedResult<TicketViewModel>> GetTicketsAsync(int page, int pageSize, TicketFilterModel? filter = null);
    Task<PagedResult<TicketViewModel>> SearchTicketsAsync(string query, int page, int pageSize);
    Task<ApiResultWrapper<TicketCommentDto>> AddTicketCommentAsync(Guid ticketId, string content);
    Task<List<TicketCommentDto>> GetTicketCommentsAsync(Guid ticketId);
}
