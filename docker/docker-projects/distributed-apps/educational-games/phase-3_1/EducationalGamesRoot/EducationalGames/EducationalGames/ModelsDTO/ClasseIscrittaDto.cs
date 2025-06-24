namespace EducationalGames.ModelsDTO;

// DTO per visualizzare una classe a cui lo studente è iscritto
// Include dettagli della classe e l'elenco dei giochi associati
public record ClasseIscrittaDto(
    int IdClasse,
    string NomeClasse,
    string MateriaNome,
    string NomeDocente, // Nome completo del docente
    List<GiocoDto> GiochiDisponibili // Lista dei giochi associati a questa classe
                                     // Potremmo aggiungere DataIscrizione se utile
);
