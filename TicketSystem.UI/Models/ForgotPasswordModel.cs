using System.ComponentModel.DataAnnotations;

namespace TicketSystem.UI.Models;

public sealed class ForgotPasswordModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
