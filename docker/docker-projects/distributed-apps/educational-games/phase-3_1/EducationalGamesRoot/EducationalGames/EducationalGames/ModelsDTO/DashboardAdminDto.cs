namespace EducationalGames.ModelsDTO;

// --- NUOVO DTO per Dashboard Admin (Fase 9) ---
public record DashboardAdminDto(
    int TotaleUtenti,
    int TotaleDocenti,
    int TotaleStudenti,
    int TotaleClassi,
    int TotaleGiochi,
    int TotaleArgomenti,
    int TotaleMaterie
// Aggiungere altre statistiche globali se necessario
);