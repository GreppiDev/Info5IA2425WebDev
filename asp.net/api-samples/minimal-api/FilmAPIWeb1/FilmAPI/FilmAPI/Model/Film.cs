using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmAPI.Model;

public class Film
{
	//FILM(Id, Titolo, DataProduzione, RegistaId*, Durata)
	public int Id { get; set; }
	public string Titolo { get; set; }=null!;
	public DateTime DataProduzione { get; set; }
	//foreign key
	public int RegistaId { get; set; }
	//navigational property
	public Regista Regista { get; set; }=null!;
	//durata in minuti
	public int Durata { get; set; }
	public List<Proiezione> Proiezioni { get; set; } = [];
	public List<Cinema> Cinemas { get; set; }= [];

}
