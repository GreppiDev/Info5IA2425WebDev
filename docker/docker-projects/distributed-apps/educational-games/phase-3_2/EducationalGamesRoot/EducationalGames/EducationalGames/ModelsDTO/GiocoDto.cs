namespace EducationalGames.ModelsDTO;

// DTO per rappresentare un Gioco nella lista del catalogo
public record GiocoDto(
    int Id,
    string Titolo,
    string? DescrizioneBreve,
    uint MaxMonete, // Manteniamo uint come nel modello
    string? Immagine1, // Immagine principale opzionale
    List<ArgomentoDto> Argomenti // Lista degli argomenti associati
);
