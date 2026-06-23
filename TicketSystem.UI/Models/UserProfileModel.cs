namespace TicketSystem.UI.Models;

public sealed class UserProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
