// File: wwwroot/js/navbar.js

/**
 * Aggiorna la visibilità degli elementi della navbar in base ai dati utente.
 * @param {object | null} userData - L'oggetto dati utente restituito dall'API (es. da /my-roles) o null se non loggato.
 */
function updateNavbar(userData) {
  console.log("[updateNavbar] Called with userData:", userData);

  // Riferimenti Navbar (ottenuti qui per assicurarsi che esistano quando la funzione viene chiamata)
  const navLogin = document.getElementById("nav-login");
  const navRegister = document.getElementById("nav-register");
  const navProfile = document.getElementById("nav-profile");
  const navLogout = document.getElementById("nav-logout");
  const navUsernameSpan = document.getElementById("nav-username");
  const navAdminDashboard = document.getElementById("nav-admin-dashboard");
  const navStudenteClassi = document.getElementById("nav-studente-classi");
  const navDocenteGestione = document.getElementById("nav-docente-gestione");

  // Log elementi trovati (utile per debug)
  console.log("[updateNavbar] Elements found:", {
    navLogin: !!navLogin,
    navRegister: !!navRegister,
    navProfile: !!navProfile,
    navLogout: !!navLogout,
    navUsernameSpan: !!navUsernameSpan,
    navAdminDashboard: !!navAdminDashboard,
    navStudenteClassi: !!navStudenteClassi,
    navDocenteGestione: !!navDocenteGestione,
  });

  // Determina lo stato di login dai dati forniti (gestisce userData null)
  const isLoggedIn = !!userData; // Vero se userData non è null/undefined
  const username = userData?.username; // Usa optional chaining

  console.log("[updateNavbar] Determined display state:", {
    isLoggedIn,
    username,
  });

  // Applica visibilità base
  if (isLoggedIn) {
    console.log("[updateNavbar] Setting state for LOGGED IN user.");
    if (navProfile) navProfile.classList.remove("d-none");
    else console.warn("navProfile not found");
    if (navLogout) navLogout.classList.remove("d-none");
    else console.warn("navLogout not found");
    if (navUsernameSpan && username) navUsernameSpan.textContent = username;
    if (navLogin) navLogin.classList.add("d-none");
    else console.warn("navLogin not found");
    if (navRegister) navRegister.classList.add("d-none");
    else console.warn("navRegister not found");
  } else {
    console.log("[updateNavbar] Setting state for LOGGED OUT user.");
    if (navLogin) navLogin.classList.remove("d-none");
    else console.warn("navLogin not found");
    if (navRegister) navRegister.classList.remove("d-none");
    else console.warn("navRegister not found");
    if (navProfile) navProfile.classList.add("d-none");
    else console.warn("navProfile not found");
    if (navLogout) navLogout.classList.add("d-none");
    else console.warn("navLogout not found");
    if (navUsernameSpan) navUsernameSpan.textContent = "";
  }

  // Aggiorna link specifici per ruolo (basati su userData, gestisce null)
  const hasAdminRole = userData?.isAdmin ?? false;
  const hasDocenteRole = userData?.isDocente ?? false;
  const hasStudenteRole = userData?.isStudente ?? false;
  console.log("[updateNavbar] Role flags:", {
    hasAdminRole,
    hasDocenteRole,
    hasStudenteRole,
  });

  if (navAdminDashboard) {
    if (hasAdminRole) navAdminDashboard.classList.remove("d-none");
    else navAdminDashboard.classList.add("d-none");
  } else console.warn("navAdminDashboard not found");

  if (navDocenteGestione) {
    if (hasDocenteRole) navDocenteGestione.classList.remove("d-none");
    else navDocenteGestione.classList.add("d-none");
  } else console.warn("navDocenteGestione not found");

  if (navStudenteClassi) {
    if (hasStudenteRole) navStudenteClassi.classList.remove("d-none");
    else navStudenteClassi.classList.add("d-none");
  } else console.warn("navStudenteClassi not found");

  console.log("[updateNavbar] Finished applying styles.");
}

/**
 * Esegue il logout tramite POST e reindirizza alla home.
 */
function postLogout() {
  // Non è più necessario pulire sessionStorage
  fetch("/api/account/logout", { method: "POST" }).finally(() => {
    console.log("Logout request sent, redirecting to home page.");
    window.location.href = "/"; // Reindirizza alla home page
  });
}

// --- RIMOSSO event listener DOMContentLoaded da navbar.js ---
// La chiamata a fetch e updateNavbar verrà fatta dallo script della pagina host.
