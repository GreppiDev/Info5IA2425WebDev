using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EducationalGames.Models; 

namespace EducationalGames.Utils
{
    public static class RoleUtils
    {
        /// <summary>
        /// Ottiene i ruoli in cascata per un dato ruolo utente.
        /// </summary>
        /// <param name="role">Il ruolo utente di partenza.</param>
        /// <returns>Un array di RuoloUtente che include il ruolo di partenza e quelli inferiori.</returns>
        public static RuoloUtente[] GetCascadedRolesEnum(RuoloUtente role)
        {
            return role switch
            {
                RuoloUtente.Admin => [RuoloUtente.Admin, RuoloUtente.Docente, RuoloUtente.Studente],
                RuoloUtente.Docente => [RuoloUtente.Docente, RuoloUtente.Studente],
                RuoloUtente.Studente => [RuoloUtente.Studente],
                _ => [] // Caso di default, restituisce un array vuoto
            };
        }

        /// <summary>
        /// Ottiene i ruoli in cascata (come stringhe) per un dato ruolo utente.
        /// Utile per la creazione dei Claims.
        /// </summary>
        /// <param name="role">Il ruolo utente di partenza.</param>
        /// <returns>Un array di stringhe che rappresentano i ruoli in cascata.</returns>
        public static string[] GetCascadedRoles(RuoloUtente role)
        {
            // Chiama il metodo che lavora con gli enum e converte il risultato in stringhe
            return [.. GetCascadedRolesEnum(role).Select(r => r.ToString())];
        }


        /// <summary>
        /// Ottiene i ruoli in cascata (come stringhe) direttamente dai Claims dell'utente.
        /// </summary>
        /// <param name="user">L'oggetto ClaimsPrincipal dell'utente autenticato.</param>
        /// <returns>Un array di stringhe che rappresentano i ruoli in cascata, o un array vuoto se il claim del ruolo non è presente o non valido.</returns>
        public static string[] GetCascadedRolesFromClaims(ClaimsPrincipal user)
        {
            // Trova il claim del ruolo
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim == null)
            {
                return []; // Nessun claim di ruolo trovato
            }

            // Prova a convertire la stringa del claim nell'enum RuoloUtente
            if (Enum.TryParse<RuoloUtente>(roleClaim, true, out var userRole)) // true per ignorare maiuscole/minuscole
            {
                // Se la conversione ha successo, ottieni i ruoli in cascata
                return GetCascadedRoles(userRole);
            }

            // Se la stringa del claim non corrisponde a nessun valore dell'enum
            return [];
        }
    }
}
