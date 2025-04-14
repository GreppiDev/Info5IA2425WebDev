using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class AziendaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Indirizzo { get; set; }
    public AziendaDTO() { }
    public AziendaDTO(Azienda azienda) =>
    (Id, Nome, Indirizzo) = (azienda.Id, azienda.Nome, azienda.Indirizzo);
}

