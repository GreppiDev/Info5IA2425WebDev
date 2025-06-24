using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("CLASSI_VIRTUALI")]
    public class ClasseVirtuale
    {
        [Key]
        [Column("ID_Classe")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NomeClasse")]
        public string Nome { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("CodiceIscrizione")]
        public string CodiceIscrizione { get; set; } = null!;

        [Required]
        [Column("ID_Docente")]
        public int DocenteId { get; set; }

        [Required]
        [Column("ID_Materia")]
        public int MateriaId { get; set; }

        // Navigation properties
        [ForeignKey("DocenteId")]
        public virtual Utente Docente { get; set; } = null!;
        [ForeignKey("MateriaId")]
        public virtual Materia Materia { get; set; } = null!;
        public virtual ICollection<Iscrizione> Iscrizioni { get; set; } = [];
        public virtual ICollection<ProgressoStudente> Progressi { get; set; } = [];

        // --- SKIP NAVIGATION PROPERTY ---
        public virtual ICollection<Utente> StudentiIscritti { get; set; } = [];

        // --- SKIP NAVIGATION PROPERTY ---
        public virtual ICollection<Videogioco> Giochi { get; set; } = [];

        // --- MANTENUTA NAVIGATION PROPERTY ALLA TABELLA DI JOIN ---
        public virtual ICollection<ClasseGioco> ClassiGiochi { get; set; } = [];
    }
}
