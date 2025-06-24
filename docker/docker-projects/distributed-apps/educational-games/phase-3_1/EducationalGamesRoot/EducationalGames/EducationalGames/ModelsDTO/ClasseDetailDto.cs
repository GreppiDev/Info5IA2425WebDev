namespace EducationalGames.ModelsDTO;

// DTO per i dettagli di una classe
// Include l'elenco dei giochi già associati
public record ClasseDetailDto(
    int Id,
    string Nome,
    string MateriaNome,
    string NomeDocente, // Nome completo del docente
    string CodiceIscrizione,
    List<GiocoDto> GiochiAssociati // Usa il GiocoDto definito nella Fase 1
                                   // Potremmo aggiungere anche la lista studenti se necessario
);