using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;

// DTO per ricevere il codice di iscrizione dal frontend
public record IscrivitiDto(
    [Required(ErrorMessage = "Il codice di iscrizione è obbligatorio.")]
    [StringLength(20, ErrorMessage = "Il codice iscrizione non è valido.")] // Lunghezza max definita nel modello ClasseVirtuale
    string CodiceIscrizione
);