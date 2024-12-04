using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class ProdottoDTO
{
    public int ProdottoId { get; set; }
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descrizione { get; set; }
    public ProdottoDTO() { }
    public ProdottoDTO(Prodotto prodotto) =>
    (ProdottoId, AziendaId, Nome, Descrizione) = (prodotto.Id, prodotto.AziendaId, prodotto.Nome, prodotto.Descrizione);

}

