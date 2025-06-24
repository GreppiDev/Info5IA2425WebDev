namespace EducationalGames.ModelsDTO;

// Riepilogo di una classe per la dashboard studente
public record ClasseIscrittaRiepilogoDto(
    int IdClasse,
    string NomeClasse,
    string NomeDocente,
    int NumeroGiochiTotali, // Totale giochi assegnati alla classe
    int NumeroGiochiCompletati // Giochi in cui lo studente ha raggiunto MaxMonete (o una soglia)
                               // Potremmo aggiungere: uint MoneteRaccolteInClasse
);