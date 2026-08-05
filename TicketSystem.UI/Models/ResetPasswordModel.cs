using System.ComponentModel.DataAnnotations;

namespace TicketSystem.UI.Models;

public sealed class ResetPasswordModel
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}
