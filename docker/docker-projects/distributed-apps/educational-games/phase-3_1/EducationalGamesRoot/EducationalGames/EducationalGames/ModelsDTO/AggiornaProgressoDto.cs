using System.ComponentModel.DataAnnotations;

namespace EducationalGames.ModelsDTO;


    // DTO per ricevere i dati di aggiornamento del progresso dal frontend
    public record AggiornaProgressoDto(
        [Required(ErrorMessage = "ID Gioco mancante.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID Gioco non valido.")]
        int GiocoId,

        [Required(ErrorMessage = "ID Classe mancante.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID Classe non valido.")]
        int ClasseId,

        [Required(ErrorMessage = "Punteggio (Monete) mancante.")]
        [Range(0, uint.MaxValue, ErrorMessage = "Le monete non possono essere negative.")] // Usa uint come nel modello
        uint MoneteRaccolte // Nome corrisponde a ProgressoStudente.MoneteRaccolte
    );
