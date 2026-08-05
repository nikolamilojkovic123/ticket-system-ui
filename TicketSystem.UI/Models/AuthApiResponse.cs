using TicketSystem.UI.Enums;

namespace TicketSystem.UI.Models;

public sealed class AuthApiResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthTokenResponse? Data { get; set; }
}

public sealed class AuthTokenResponse
{
    public string Token { get; set; } = default!;
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
}
