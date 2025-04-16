using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("ARGOMENTI")]
    public class Argomento
    {
        [Key]
        [Column("ID_Argomento")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NomeArgomento")]
        public string Nome { get; set; } = null!;

        // --- SKIP NAVIGATION PROPERTY ---
        public virtual ICollection<Videogioco> Videogiochi { get; set; } = [];

        // --- MANTENUTA NAVIGATION PROPERTY ALLA TABELLA DI JOIN ---
        public virtual ICollection<GiocoArgomento> GiochiArgomenti { get; set; } = [];
    }
}
