using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Azienda
{
    public int Id { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string Nome { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string? Indirizzo { get; set; }
    public List<Prodotto> Prodotti { get; set; } =null!;
    public List<Sviluppatore> Sviluppatori { get; set; } = null!;
}

