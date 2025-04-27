using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

// DTO per ricevere i dati per creare una nuova classe
public record CreaClasseDto(
    [Required(ErrorMessage = "Il nome della classe è obbligatorio.")]
        [StringLength(50, ErrorMessage = "Il nome della classe non può superare i 50 caratteri.")]
        string NomeClasse,

    [Required(ErrorMessage = "Selezionare una materia è obbligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selezionare una materia valida.")]
        int MateriaId
);
