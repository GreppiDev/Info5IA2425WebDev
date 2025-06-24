using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

// --- DTO Specifico per Creazione/Modifica Gioco (da mettere in ModelsDTO/GameDtos.cs) ---
public record GiocoCreateUpdateDto(
    [Required] string Titolo,
    string? DescrizioneBreve,
    string? DescrizioneEstesa,
    [Range(0, uint.MaxValue)] uint MaxMonete,
    string? Immagine1,
    string? Immagine2,
    string? Immagine3,
    string? DefinizioneGioco, // JSON come stringa
    List<int>? ArgomentiId // Lista degli ID degli argomenti da associare
);