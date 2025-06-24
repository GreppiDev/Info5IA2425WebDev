namespace EducationalGames.ModelsDTO;

// DTO principale per la dashboard del docente
public record DashboardDocenteDto(
    int TotaleClassi,
    int TotaleStudentiDistinti, // Numero di studenti unici iscritti alle classi del docente
    List<ClasseRiepilogoDto> UltimeClassiModificate, // Esempio: le 5 più recenti o attive
    List<StudenteAttivitaDto> StudentiPiuAttivi // Esempio: i 5 con più monete o attività recente
                                                // Aggiungere altre statistiche se necessario
);