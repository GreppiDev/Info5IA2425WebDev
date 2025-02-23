using System.ComponentModel.DataAnnotations;

namespace ProtectedAPI.Model;

public class AdminCredentialsOptions
{
    [Required(ErrorMessage = "Admin email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; } = string.Empty;
}