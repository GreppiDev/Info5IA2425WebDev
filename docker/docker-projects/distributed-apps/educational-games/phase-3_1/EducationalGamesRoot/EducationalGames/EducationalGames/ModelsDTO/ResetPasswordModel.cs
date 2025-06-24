using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

public record ResetPasswordModel
{
    [Required]
    public required string Token { get; init; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; init; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Le password non coincidono.")]
    public required string ConfirmPassword { get; init; }
}