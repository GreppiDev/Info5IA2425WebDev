using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    // Enum per i ruoli utente
    public enum RuoloUtente
    {
        Admin,
        Docente,
        Studente
    }

    [Table("UTENTI")] 
    public class Utente
    {
        [Key] // Chiave primaria
        [Column("ID_Utente")] 
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio.")] // Campo obbligatorio
        [StringLength(50, ErrorMessage = "Il nome non può superare i 50 caratteri.")] // Lunghezza massima
        public string Nome { get; set; } = null!; 

        [Required(ErrorMessage = "Il cognome è obbligatorio.")]
        [StringLength(50, ErrorMessage = "Il cognome non può superare i 50 caratteri.")]
        public string Cognome { get; set; } = null!;

        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [StringLength(100, ErrorMessage = "L'email non può superare i 100 caratteri.")]
        [EmailAddress(ErrorMessage = "Formato email non valido.")] // Validazione formato email
        public string Email { get; set; } = null!;

        [Required] // L'hash è obbligatorio (anche se fittizio per login esterni)
        [StringLength(255)] // Lunghezza adeguata per hash comuni (es. bcrypt)
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [Column("Ruolo")]
        public RuoloUtente Ruolo { get; set; }

        [Required] 
        [Column("EmailVerificata")] 
        public bool EmailVerificata { get; set; } = false; // Default a false per nuovi utenti

        [StringLength(100)] // Lunghezza adeguata per un token (es. GUID)
        [Column("TokenVerificaEmail")] 
        public string? TokenVerificaEmail { get; set; } // Nullable: il token viene rimosso dopo la verifica

        [Column("ScadenzaTokenVerificaEmail")] 
        public DateTime? ScadenzaTokenVerificaEmail { get; set; } // Nullable: non c'è scadenza se già verificato o token non generato

        [StringLength(100)] // Lunghezza simile al token di verifica
        [Column("TokenResetPassword")]
        public string? TokenResetPassword { get; set; } // Nullable

        [Column("ScadenzaTokenResetPassword")]
        public DateTime? ScadenzaTokenResetPassword { get; set; } // Nullable

        // Navigation properties 
        public virtual ICollection<ClasseVirtuale> ClassiCreate { get; set; } = []; 
        public virtual ICollection<Iscrizione> Iscrizioni { get; set; } = [];
        public virtual ICollection<ProgressoStudente> Progressi { get; set; } = [];

        // Navigation property "skip" per ottenere le classi a cui uno studente è iscritto
        public virtual ICollection<ClasseVirtuale> ClassiIscritte { get; set; } = [];
    }
}