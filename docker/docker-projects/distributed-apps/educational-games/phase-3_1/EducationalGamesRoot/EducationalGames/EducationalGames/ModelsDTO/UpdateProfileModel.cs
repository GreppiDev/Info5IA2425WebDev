using System.ComponentModel.DataAnnotations;
using EducationalGames.Models;

namespace EducationalGames.ModelsDTO;

public record UpdateProfileModel(
    [Required(ErrorMessage = "Il nome è obbligatorio.")]
    [StringLength(50)]
    string Nome,

    [Required(ErrorMessage = "Il cognome è obbligatorio.")]
    [StringLength(50)]
    string Cognome,

    [Required(ErrorMessage = "Il ruolo è obbligatorio.")]
    [EnumDataType(typeof(RuoloUtente), ErrorMessage = "Ruolo non valido.")]
    [RegularExpression("^(Studente|Docente)$", ErrorMessage = "Il ruolo può essere solo Studente o Docente.")]
    RuoloUtente Ruolo
);
