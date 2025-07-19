using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("GIOCHI_ARGOMENTI")]
    public class GiocoArgomento
    {
        [Key] // Part of composite key
        [Column("ID_Gioco")]
        public int GiocoId { get; set; }

        [Key] // Part of composite key
        [Column("ID_Argomento")]
        public int ArgomentoId { get; set; }

        // Navigation properties
        [ForeignKey("GiocoId")]
        public virtual Videogioco Gioco { get; set; } = null!;
        [ForeignKey("ArgomentoId")]
        public virtual Argomento Argomento { get; set; } = null!;
    }
}
