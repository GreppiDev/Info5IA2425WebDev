using System;
using FilmAPI.Model;

namespace FilmAPI.ModelDTO;

public class ProiezioneDTO
{
	public int Id { get; set; }
	public int CinemaId { get; set; }
	public int FilmId { get; set; }
	public DateOnly Data { get; set; }
	public TimeOnly Ora { get; set; }
	
	public ProiezioneDTO()
	{
		
	}
	public ProiezioneDTO(Proiezione p)
	{
		(Id,CinemaId, FilmId, Data, Ora)= (p.Id,p.CinemaId, p.FilmId, p.Data, p.Ora);
	}
}
