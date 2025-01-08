using System;
using FilmAPI.Model;

namespace FilmAPI.ModelDTO;

public class FilmDTO
{
	public int Id { get; set; }
	public string Titolo { get; set; } = null!;
	public DateTime DataProduzione { get; set; }
	//foreign key
	public int RegistaId { get; set; }
	public int Durata { get; set; }
	//due costruttori: uno vuoto e uno che prende come parametro un oggetto Film
	public FilmDTO()
	{
		
	}
	public FilmDTO(Film film)
	{
		(Id, Titolo, DataProduzione, RegistaId, Durata)= (film.Id, film.Titolo, film.DataProduzione, film.RegistaId, film.Durata);
	}

}
