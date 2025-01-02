using System;

namespace FilmAPI.Model;

public class Cinema
{
	//CINEMA(Id, Nome, Indirizzo, Città)
	public int Id { get; set; }
	public string Nome { get; set; }=null!;
	public string Indirizzo { get; set; }=null!;
	public string Città { get; set; }=null!;
	public List<Proiezione> Proiezioni { get; set; }=null!;
	public List<Film> Films { get; set; }=null!;
	
}
