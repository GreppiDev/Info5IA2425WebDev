using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("PROGRESSI_STUDENTI")]
    public class ProgressoStudente
    {
        [Key] // Part of composite key
        [Column("ID_Studente")]
        public int StudenteId { get; set; }

        [Key] // Part of composite key
        [Column("ID_Gioco")]
        public int GiocoId { get; set; }

        [Key] // Part of composite key
        [Column("ID_Classe")]
        public int ClasseId { get; set; }

        [Required]
        [Column("MoneteRaccolte")]
        public uint MoneteRaccolte { get; set; } = 0; // Use uint for UNSIGNED

        [Column("UltimoAggiornamento")]
        public DateTime UltimoAggiornamento { get; set; } // Default/Update handled by DB

        // Navigation properties
        [ForeignKey("StudenteId")]
        public virtual Utente Studente { get; set; } = null!;
        [ForeignKey("GiocoId")]
        public virtual Videogioco Gioco { get; set; } = null!;
        [ForeignKey("ClasseId")]
        public virtual ClasseVirtuale Classe { get; set; } = null!;
    }
}
