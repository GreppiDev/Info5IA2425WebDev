using System;

namespace FilmAPI.Model;

public class Proiezione
{
	//PROIEZIONE(Id, CinemaId*, FilmId*, Data, Ora)
	//l'Id è la chiave primaria
	public int Id { get; set; }
	public int CinemaId { get; set; }
	public Cinema Cinema { get; set; }=null!;
	public int FilmId { get; set; }
	public Film Film { get; set; } = null!;
	public DateOnly Data { get; set; }
	public TimeOnly Ora { get; set; }
	
}
