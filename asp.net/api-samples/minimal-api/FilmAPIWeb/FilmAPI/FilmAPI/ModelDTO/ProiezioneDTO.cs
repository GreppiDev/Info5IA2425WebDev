using System;
using FilmAPI.Model;

namespace FilmAPI.ModelDTO;

public class ProiezioneDTO
{
	public int CinemaId { get; set; }
	public int FilmId { get; set; }
	public DateOnly Data { get; set; }
	public TimeOnly Ora { get; set; }
	
	public ProiezioneDTO()
	{
		
	}
	public ProiezioneDTO(Proiezione p)
	{
		(CinemaId, FilmId, Data, Ora)= (p.CinemaId, p.FilmId, p.Data, p.Ora);
	}
}
