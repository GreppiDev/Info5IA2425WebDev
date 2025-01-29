using System;
using FilmAPI.Model;

namespace FilmAPI.ModelDTO;

public class RegistaDTO
{
	    public int Id { get; set; }
	    public string Nome { get; set; } = null!;
	    public string Cognome { get; set; } = null!;
	    public string Nazionalità { get; set; } = null!;
	    public int? TmdbId { get; set; }
	
	    public RegistaDTO()
	    {
	    }
	
	    public RegistaDTO(Regista regista)
	    {
	        (Id, Nome, Cognome, Nazionalità, TmdbId) =
	            (regista.Id, regista.Nome, regista.Cognome, regista.Nazionalità, regista.TmdbId);
	    }

}
