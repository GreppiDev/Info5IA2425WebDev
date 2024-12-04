using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class AziendaDTO
{
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Indirizzo { get; set; }
    public AziendaDTO() { }
    public AziendaDTO(Azienda azienda) =>
    (AziendaId, Nome, Indirizzo) = (azienda.Id, azienda.Nome, azienda.Indirizzo);
}

