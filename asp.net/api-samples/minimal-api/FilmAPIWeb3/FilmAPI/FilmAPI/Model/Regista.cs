using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Model;

[Index(nameof(TmdbId), IsUnique = true)]
public class Regista
{
    //REGISTA(Id, Nome, Cognome, Nazionalità)
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Cognome { get; set; } = null!;
    public string Nazionalità { get; set; } = null!;
    public int? TmdbId { get; set; }
    //collection property
    //1 regista ---> n film
    public List<Film> Films { get; set; } = [];
}
