namespace EducationalGames.ModelsDTO;

// DTO per restituire i dati necessari per avviare/giocare un gioco
// Potrebbe essere restituito da /api/giochi/{idGioco}/play
public record GiocoPlayDto(
    int Id,
    string Titolo,
    uint MaxMonete,
    string? DefinizioneGioco, // La definizione JSON del gioco/quiz
    string? UrlEsterno // URL se il gioco è ospitato esternamente (alternativa a DefinizioneGioco)
                       // Aggiungere altri campi se necessario (es. DescrizioneEstesa)
);
