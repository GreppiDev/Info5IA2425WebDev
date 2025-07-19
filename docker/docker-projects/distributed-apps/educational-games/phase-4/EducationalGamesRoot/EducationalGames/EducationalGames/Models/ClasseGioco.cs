using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("CLASSI_GIOCHI")]
    public class ClasseGioco
    {
        [Key] // Part of composite key
        [Column("ID_Classe")]
        public int ClasseId { get; set; }

        [Key] // Part of composite key
        [Column("ID_Gioco")]
        public int GiocoId { get; set; }

        // Navigation properties
        [ForeignKey("ClasseId")]
        public virtual ClasseVirtuale Classe { get; set; } = null!;
        [ForeignKey("GiocoId")]
        public virtual Videogioco Gioco { get; set; } = null!;
    }
}
