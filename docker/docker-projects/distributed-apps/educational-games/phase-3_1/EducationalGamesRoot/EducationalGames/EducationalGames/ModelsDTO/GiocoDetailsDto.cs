namespace EducationalGames.ModelsDTO;

// DTO opzionale più dettagliato, se necessario in futuro
// per visualizzare/modificare un singolo gioco

public record GiocoDetailDto(
    int Id,
    string Titolo,
    string? DescrizioneBreve,
    string? DescrizioneEstesa,
    uint MaxMonete,
    string? Immagine1,
    string? Immagine2,
    string? Immagine3,
    string? DefinizioneGioco, // Oppure un tipo deserializzato
    List<ArgomentoDto> Argomenti
);

