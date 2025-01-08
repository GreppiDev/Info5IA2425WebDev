using System;
using FilmAPI.Model;

namespace FilmAPI.ModelDTO;

public class CinemaDTO
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
	public string Indirizzo { get; set; } = null!;
	public string Citta { get; set; } = null!;

	public CinemaDTO()
	{

	}
	public CinemaDTO(Cinema c)
	{
		(Id, Nome, Indirizzo, Citta) = (c.Id, c.Nome, c.Indirizzo, c.Città);
	}
}
