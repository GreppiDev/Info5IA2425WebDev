using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

// --- NUOVO: DTO per associare un gioco a una classe ---
public record AssociaGiocoDto(
    [Required(ErrorMessage = "ID Gioco mancante.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID Gioco non valido.")]
    int GiocoId
);
