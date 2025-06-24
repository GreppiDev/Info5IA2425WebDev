using System;
using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    // Aggiunto per gestire la checkbox "Ricordami" nel form di login
    public bool RememberMe { get; set; }
}