using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

public record ResendVerificationModel([Required][EmailAddress] string Email);
