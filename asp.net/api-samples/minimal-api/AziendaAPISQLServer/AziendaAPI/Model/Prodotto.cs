using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Prodotto
{
	public int Id { get; set; }
	public int AziendaId { get; set; }
	public Azienda Azienda { get; set; } = null!;

	[Column(TypeName = "nvarchar(100)")]
	public string Nome { get; set; } = null!;

	[Column(TypeName = "nvarchar(200)")]
	public string? Descrizione { get; set; }
	//collection property
	public List<SviluppaProdotto> SviluppaProdotti { get; set; } = null!;
	//skip navigation property
	public List<Sviluppatore> Sviluppatori { get; set; } = null!;

}

