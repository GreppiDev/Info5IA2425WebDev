namespace EducationalGames.ModelsDTO;

// DTO per rappresentare una singola voce nella classifica
public record ClassificaEntryDto(
    int StudenteId,
    string NomeStudente, // Nome completo dello studente
    uint Monete // Monete totali (per gioco o per classe)
                // Potremmo aggiungere Rank se calcolato lato server
                // int? Rank
);
