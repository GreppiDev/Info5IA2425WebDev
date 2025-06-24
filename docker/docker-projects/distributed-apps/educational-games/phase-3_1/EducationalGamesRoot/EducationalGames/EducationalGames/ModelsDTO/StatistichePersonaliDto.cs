namespace EducationalGames.ModelsDTO;

// Statistiche personali dello studente
public record StatistichePersonaliDto(
    uint MoneteTotaliGuadagnate, // Somma di tutte le monete in tutte le classi
    int GiochiCompletatiTotali, // Conteggio totale giochi completati
    int ClassiIscritte // Numero totale di classi a cui è iscritto
);
