using System;

namespace FilmAPI.Model;

public class Regista
{
	//REGISTA(Id, Nome, Cognome, Nazionalità) 
	public int Id { get; set; }
	public string Nome { get; set; }=null!;
	public string Cognome { get; set; }=null!;
	public string Nazionalità { get; set; }=null!;
	//collection property
	//1 regista ---> n film
	public List<Film> Films {get; set;}=null!;
}
