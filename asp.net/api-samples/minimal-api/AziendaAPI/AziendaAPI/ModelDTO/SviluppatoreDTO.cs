using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class SviluppatoreDTO
{
    public int Id { get; set; }
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string Cognome { get; set; } = null!;
    public SviluppatoreDTO() { }
    public SviluppatoreDTO(Sviluppatore sviluppatore) =>
    (Id, AziendaId, Nome, Cognome) = (sviluppatore.Id, sviluppatore.AziendaId, sviluppatore.Nome, sviluppatore.Cognome);
}

