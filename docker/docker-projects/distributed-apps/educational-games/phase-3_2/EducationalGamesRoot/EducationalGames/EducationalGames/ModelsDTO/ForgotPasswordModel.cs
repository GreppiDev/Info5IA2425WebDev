using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

public record ForgotPasswordModel([Required][EmailAddress] string Email);
