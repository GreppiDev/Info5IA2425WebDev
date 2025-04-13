using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("MATERIE")]
    public class Materia
    {
        [Key]
        [Column("ID_Materia")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NomeMateria")]
        public string Nome { get; set; } = null!;

        // Navigation property
        public virtual ICollection<ClasseVirtuale> ClassiVirtuali { get; set; } = [];
    }
}
