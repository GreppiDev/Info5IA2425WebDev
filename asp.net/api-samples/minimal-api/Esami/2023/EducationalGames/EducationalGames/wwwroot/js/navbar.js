// File: wwwroot/js/navbar.js

/**
 * Aggiorna la visibilità degli elementi della navbar.
 * @param {object | null} userData - L'oggetto dati utente o null.
 */
function updateNavbar(userData) {
  console.log("[updateNavbar] Called with userData:", userData);

  // Riferimenti Navbar
  const navLogin = document.getElementById("nav-login");
  const navRegister = document.getElementById("nav-register");
  const navProfile = document.getElementById("nav-profile");
  const navLogout = document.getElementById("nav-logout");
  const navUsernameSpan = document.getElementById("nav-username");
  const navAdminDashboard = document.getElementById("nav-admin-dashboard");
  const navStudenteClassi = document.getElementById("nav-studente-classi");
  const navDocenteGestione = document.getElementById("nav-docente-gestione");
  const navCatalogoGiochi = document.getElementById("nav-catalogo-giochi");

  // Verifica esistenza elementi
  const elements = {
    navLogin,
    navRegister,
    navProfile,
    navLogout,
    navUsernameSpan,
    navAdminDashboard,
    navStudenteClassi,
    navDocenteGestione,
    navCatalogoGiochi,
  };
  for (const key in elements) {
    if (!elements[key]) {
      console.warn(`[updateNavbar] Element ${key} not found.`);
    }
  }

  const isLoggedIn = !!userData;
  const username = userData?.username;
  console.log("[updateNavbar] Determined display state:", {
    isLoggedIn,
    username,
  });

  // Applica visibilità base
  if (isLoggedIn) {
    if (navProfile) navProfile.classList.remove("d-none");
    if (navLogout) navLogout.classList.remove("d-none");
    if (navUsernameSpan && username) navUsernameSpan.textContent = username;
    if (navLogin) navLogin.classList.add("d-none");
    if (navRegister) navRegister.classList.add("d-none");
  } else {
    if (navLogin) navLogin.classList.remove("d-none");
    if (navRegister) navRegister.classList.remove("d-none");
    if (navProfile) navProfile.classList.add("d-none");
    if (navLogout) navLogout.classList.add("d-none");
    if (navUsernameSpan) navUsernameSpan.textContent = "";
  }

  // Aggiorna link specifici per ruolo
  const hasAdminRole = userData?.isAdmin ?? false;
  const hasDocenteRole = userData?.isDocente ?? false;
  const hasStudenteRole = userData?.isStudente ?? false;

  console.log("[updateNavbar] Role flags:", {
    hasAdminRole,
    hasDocenteRole,
    hasStudenteRole,
  });

  if (navAdminDashboard) {
    hasAdminRole
      ? navAdminDashboard.classList.remove("d-none")
      : navAdminDashboard.classList.add("d-none");
  }
  if (navDocenteGestione) {
    hasDocenteRole
      ? navDocenteGestione.classList.remove("d-none")
      : navDocenteGestione.classList.add("d-none");
  }
  if (navStudenteClassi) {
    hasStudenteRole
      ? navStudenteClassi.classList.remove("d-none")
      : navStudenteClassi.classList.add("d-none");
  }
  if (navCatalogoGiochi) {
    hasAdminRole || hasDocenteRole
      ? navCatalogoGiochi.classList.remove("d-none") // Mostra catalogo
      : navCatalogoGiochi.classList.add("d-none"); // Nascondi catalogo
  }

  console.log("[updateNavbar] Finished applying styles.");
}

/**
 * Esegue il logout tramite POST e reindirizza alla home.
 */
function postLogout() {
  fetch("/api/account/logout", { method: "POST" }).finally(() => {
    sessionStorage.removeItem("isLoggedIn");
    sessionStorage.removeItem("loggedInUser");
    console.log("Logout request sent, redirecting to home page.");
    window.location.href = "/";
  });
}

// --- Esecuzione all'avvio della pagina ---
// Lo script della pagina specifica chiamerà await TemplateLoader.initializeCommonTemplates()
// e poi updateNavbar() dopo aver fatto la sua fetch.
