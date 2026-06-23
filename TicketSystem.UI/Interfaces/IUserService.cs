using TicketSystem.UI.Models;

namespace TicketSystem.UI.Interfaces;

public interface IUserService
{
    Task<ApiResultWrapper<DocumentAudioResponseDto>> AskDocumentAsync(Stream fileStream, string fileName,
        string contentType,
        string question
        , string language);
    Task<UserProfileModel?> GetUserProfileInfoAsync();
    Task<bool> UpdateUserProfileAsync(UserProfileModel model);
    Task<ICollection<UserSelectModel>> GetUsersAsync();
    Task<List<UserStatsModel>> GetUserStatsAsync();
}
