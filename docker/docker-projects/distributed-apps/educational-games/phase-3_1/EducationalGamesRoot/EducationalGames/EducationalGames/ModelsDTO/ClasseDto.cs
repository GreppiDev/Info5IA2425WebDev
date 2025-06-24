namespace EducationalGames.ModelsDTO;

// DTO per visualizzare una classe nell'elenco del docente
public record ClasseDto(
    int Id,
    string Nome,
    string MateriaNome, // Nome della materia associata
    string CodiceIscrizione,
    int NumeroIscritti // Conteggio degli studenti iscritti
                       // Aggiungeremo qui la lista dei giochi associati nella Fase 3
);

