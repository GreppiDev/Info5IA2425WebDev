namespace EducationalGames.ModelsDTO;

// DTO per visualizzare una Materia (es. nel dropdown)
public record MateriaDto(
    int Id,
    string Nome // Corrisponde a Materia.Nome
);