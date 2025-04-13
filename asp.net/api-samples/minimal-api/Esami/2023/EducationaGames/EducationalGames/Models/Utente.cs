using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    public enum RuoloUtente
    {
        Docente,
        Studente,
        Admin
    }

    [Table("UTENTI")]
    public class Utente
    {
        [Key]
        [Column("ID_Utente")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Cognome { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = null!; // Hash of the password

        [Required]
        [Column("Ruolo")]
        public RuoloUtente Ruolo { get; set; }

        // Navigation properties
        public virtual ICollection<ClasseVirtuale> ClassiCreate { get; set; } = [];
        public virtual ICollection<Iscrizione> Iscrizioni { get; set; } = [];
        public virtual ICollection<ProgressoStudente> Progressi { get; set; } = [];

        // --- SKIP NAVIGATION PROPERTY (per Studenti) ---
        public virtual ICollection<ClasseVirtuale> ClassiIscritte { get; set; } = [];
    }
}
