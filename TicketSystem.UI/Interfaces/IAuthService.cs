namespace TicketSystem.UI.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password);
    Task<bool> ForgotPasswordAsync(string email);
    Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string token, string newPassword);
}
