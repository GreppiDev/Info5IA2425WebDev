namespace EducationalGames.ModelsDTO;

// DTO per riepilogo classe nella dashboard docente
public record ClasseRiepilogoDto(
    int Id,
    string Nome,
    string MateriaNome,
    int NumeroIscritti,
    int NumeroGiochi // Numero di giochi associati alla classe
);
