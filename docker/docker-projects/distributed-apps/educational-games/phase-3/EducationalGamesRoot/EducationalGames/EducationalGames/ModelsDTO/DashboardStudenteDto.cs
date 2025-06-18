namespace EducationalGames.ModelsDTO;

// DTO principale per la dashboard dello studente
public record DashboardStudenteDto(
    StatistichePersonaliDto Statistiche,
    List<ClasseIscrittaRiepilogoDto> ClassiRecenti // Esempio: le ultime N classi a cui si è iscritto o con attività recente
                                                   // Potremmo aggiungere: List<GiocoDaFareDto> ProssimiGiochi
);
