using System;

namespace FilmAPI.Model;

public class Proiezione
{
	//PROIEZIONE(CinemaId*, FilmId*, Data, Ora)
	public int CinemaId { get; set; }
	public Cinema Cinema { get; set; }=null!;
	public int FilmId { get; set; }
	public Film Film { get; set; } = null!;
	public DateOnly Data { get; set; }
	public TimeOnly Ora { get; set; }
	
}
