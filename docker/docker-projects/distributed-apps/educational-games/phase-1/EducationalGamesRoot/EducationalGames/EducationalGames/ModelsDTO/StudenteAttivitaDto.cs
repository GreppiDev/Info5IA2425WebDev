namespace EducationalGames.ModelsDTO;

// DTO per riepilogo attività studente nella dashboard docente
public record StudenteAttivitaDto(
    int StudenteId,
    string NomeCompleto,
    uint MoneteTotali, // Somma monete da tutti i giochi nelle classi del docente
    DateTime? UltimaAttivita // Data/ora dell'ultimo progresso registrato
);