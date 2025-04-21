// Riferimenti agli elementi della Navbar da mostrare/nascondere
// Assicurati che questi ID esistano nell'HTML della tua navbar
const navLogin = document.getElementById("nav-login");
const navRegister = document.getElementById("nav-register");
const navProfile = document.getElementById("nav-profile");
const navLogout = document.getElementById("nav-logout");
const navUsernameSpan = document.getElementById("nav-username");
const navAdminDashboard = document.getElementById("nav-admin-dashboard");
const navStudenteClassi = document.getElementById("nav-studente-classi");
const navDocenteGestione = document.getElementById("nav-docente-gestione");
// Riferimento al form di logout (necessario per la funzione postLogout)
const logoutForm = document.getElementById("logoutForm");

/**
 * Funzione per aggiornare la visibilità degli elementi della navbar
 * in base allo stato di autenticazione e ai dati utente.
 * @param {object | null} userData - L'oggetto dati utente restituito dall'API (es. da /my-roles) o null se non loggato.
 */
function updateNavbar(userData) {
  if (userData) {
    // --- UTENTE LOGGATO ---
    if (navProfile) navProfile.classList.remove("d-none");
    if (navLogout) navLogout.classList.remove("d-none");
    if (navUsernameSpan && userData.username)
      navUsernameSpan.textContent = userData.username;

    // Nascondi Login/Register
    if (navLogin) navLogin.classList.add("d-none");
    if (navRegister) navRegister.classList.add("d-none");

    // Mostra link basati sui ruoli/flag
    // Assicurati che l'API /my-roles restituisca isAdmin, isDocente, isStudente
    if (navAdminDashboard && userData.isAdmin)
      navAdminDashboard.classList.remove("d-none");
    else if (navAdminDashboard) navAdminDashboard.classList.add("d-none"); // Nascondi se non admin

    if (navDocenteGestione && userData.isDocente)
      navDocenteGestione.classList.remove("d-none");
    else if (navDocenteGestione) navDocenteGestione.classList.add("d-none");

    if (navStudenteClassi && userData.isStudente)
      navStudenteClassi.classList.remove("d-none");
    else if (navStudenteClassi) navStudenteClassi.classList.add("d-none");
  } else {
    // --- UTENTE NON LOGGATO ---
    if (navLogin) navLogin.classList.remove("d-none");
    if (navRegister) navRegister.classList.remove("d-none");

    // Nascondi Profilo/Logout e link specifici
    if (navProfile) navProfile.classList.add("d-none");
    if (navLogout) navLogout.classList.add("d-none");
    if (navAdminDashboard) navAdminDashboard.classList.add("d-none");
    if (navStudenteClassi) navStudenteClassi.classList.add("d-none");
    if (navDocenteGestione) navDocenteGestione.classList.add("d-none");
  }
}

/**
 * Funzione per eseguire il logout inviando una richiesta POST all'API
 * e poi reindirizzando alla home page.
 */
function postLogout() {
  fetch("/api/account/logout", {
    method: "POST",
    // Aggiungi header anti-forgery se necessario nel tuo backend
    // headers: { 'RequestVerificationToken': 'TOKEN_QUI' }
  })
    .then((response) => {
      console.log("Logout request sent, redirecting to home page.");
      window.location.href = "/"; // Reindirizza sempre alla home dopo logout
    })
    .catch((error) => {
      console.error("Error during logout fetch:", error);
      window.location.href = "/"; // Reindirizza comunque alla home
    });
}

// --- Esecuzione all'avvio della pagina ---
document.addEventListener("DOMContentLoaded", function () {
  console.log("Checking auth status for navbar...");
  // Chiama l'API per ottenere lo stato dell'utente
  fetch("/api/account/my-roles")
    .then((response) => {
      if (response.ok) {
        return response.json(); // Utente loggato, ottieni i dati
      } else {
        // Utente non loggato o errore API
        return null; // Passa null a updateNavbar
      }
    })
    .then((data) => {
      // Aggiorna la navbar in base ai dati (o null)
      updateNavbar(data);
    })
    .catch((error) => {
      // Errore fetch, assumi utente non loggato
      console.error("Error fetching user status for navbar:", error);
      updateNavbar(null);
    });
});
