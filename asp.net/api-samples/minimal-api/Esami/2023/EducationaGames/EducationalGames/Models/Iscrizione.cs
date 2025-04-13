using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("ISCRIZIONI")]
    public class Iscrizione
    {
        [Key] // Part of composite key
        [Column("ID_Studente")]
        public int StudenteId { get; set; }

        [Key] // Part of composite key
        [Column("ID_Classe")]
        public int ClasseId { get; set; }

        [Column("DataIscrizione")]
        public DateTime DataIscrizione { get; set; } = DateTime.UtcNow; // Default value handled by DB

        // Navigation properties
        [ForeignKey("StudenteId")]
        public virtual Utente Studente { get; set; } = null!;
        [ForeignKey("ClasseId")]
        public virtual ClasseVirtuale Classe { get; set; } = null!;
    }
}
