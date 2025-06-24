using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationalGames.Models
{
    [Table("VIDEOGIOCHI")]
    public class Videogioco
    {
        [Key]
        [Column("ID_Gioco")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titolo { get; set; } = null!;

        [StringLength(160)]
        public string? DescrizioneBreve { get; set; }

        [Column(TypeName = "TEXT")]
        public string? DescrizioneEstesa { get; set; }

        [Required]
        [Column("MaxMonete")]
        public uint MaxMonete { get; set; } = 0; // Use uint for UNSIGNED

        [StringLength(255)]
        public string? Immagine1 { get; set; }

        [StringLength(255)]
        public string? Immagine2 { get; set; }

        [StringLength(255)]
        public string? Immagine3 { get; set; }

        // Assuming JSON maps to string for broader compatibility initially.
        // Specific DB providers (like Pomelo.EFCore.MySql) might offer better JSON support.
        [Column("DefinizioneGioco", TypeName = "json")] // Or TEXT if JSON type is not supported/desired
        public string? DefinizioneGioco { get; set; }

        // --- SKIP NAVIGATION PROPERTIES ---
        public virtual ICollection<ClasseVirtuale> ClassiVirtuali { get; set; } = [];
        public virtual ICollection<Argomento> Argomenti { get; set; } = [];

        // --- MANTENUTE NAVIGATION PROPERTIES ALLE TABELLE DI JOIN ---
        public virtual ICollection<ClasseGioco> ClassiGiochi { get; set; } = [];
        public virtual ICollection<GiocoArgomento> GiochiArgomenti { get; set; } = [];

        public virtual ICollection<ProgressoStudente> Progressi { get; set; } = [];
    }
}
