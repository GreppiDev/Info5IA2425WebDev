using System.ComponentModel.DataAnnotations;
using EducationalGames.Models; // Assicurarsi che il namespace sia corretto per RuoloUtente

namespace EducationalGames.ModelsDTO
{
    // Modello per la registrazione pubblica (Docente o Studente)
    public class RegisterModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio.")]
        [StringLength(50)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Il cognome è obbligatorio.")]
        [StringLength(50)]
        public string Cognome { get; set; } = null!;

        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Formato email non valido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere lunga almeno 8 caratteri.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Il ruolo è obbligatorio.")]
        [EnumDataType(typeof(RuoloUtente), ErrorMessage = "Ruolo non valido.")]
        // La validazione effettiva che il ruolo non sia Admin viene fatta nell'endpoint /register
        public RuoloUtente Ruolo { get; set; } // Ruolo richiesto (Docente o Studente)
    }

    // Modello per la creazione di utenti da parte dell'Admin
    // Simile a RegisterModel ma senza la restrizione sul ruolo Admin
    public class AdminCreateUserModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio.")]
        [StringLength(50)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Il cognome è obbligatorio.")]
        [StringLength(50)]
        public string Cognome { get; set; } = null!;

        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Formato email non valido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere lunga almeno 8 caratteri.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Il ruolo è obbligatorio.")]
        [EnumDataType(typeof(RuoloUtente), ErrorMessage = "Ruolo non valido.")]
        public RuoloUtente Ruolo { get; set; } // Qualsiasi ruolo può essere specificato dall'Admin
    }
}
