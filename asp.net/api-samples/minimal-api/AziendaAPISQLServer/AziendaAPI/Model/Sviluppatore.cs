using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Sviluppatore
{
	public int Id { get; set; }
	public int AziendaId { get; set; }
	public Azienda Azienda { get; set; } = null!;

	[Column(TypeName = "nvarchar(40)")]
	public string Nome { get; set; } = null!;

	[Column(TypeName = "nvarchar(40)")]
	public string Cognome { get; set; } = null!;
	//collection property
	public List<SviluppaProdotto> SviluppaProdotti { get; set; } = null!;
	//skip navigation property
	public List<Prodotto> Prodotti { get; set; } = null!;
}

