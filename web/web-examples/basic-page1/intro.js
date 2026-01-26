/*
  intro.js
  JavaScript minimale per intro.html.

  Cosa fa:
  1) Gestisce il tema chiaro/scuro
    - salva la scelta in localStorage (così resta anche dopo refresh)
    - applica il tema impostando data-theme sul <body>
    - style.css reagisce a body[data-theme="light"] cambiando le variabili CSS

  2) Gestisce piccoli comportamenti della navbar
      - quando apri un dropdown, chiude gli altri
    - quando clicchi un link, chiude hamburger e dropdown
*/

(function () {
  // Chiave usata per salvare il tema nel browser.
  // È la stessa usata nelle altre pagine, così il tema rimane coerente.
  const STORAGE_KEY = "cssTheme";

  // Elementi DOM che ci interessano
  const themeBtn = document.getElementById("themeToggle");
  const navToggle = document.getElementById("navToggle");

  // Legge il tema salvato (se presente). Se non c'è, usa dark come default.
  function getTheme() {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved === "light" || saved === "dark") return saved;
    return "dark";
  }

  // Applica il tema:
  // - data-theme sul body
  // - salvataggio in localStorage
  // - aggiornamento testo sul bottone
  function setTheme(theme) {
    document.body.setAttribute("data-theme", theme);
    localStorage.setItem(STORAGE_KEY, theme);

    if (themeBtn) {
      const label = theme === "light" ? "chiaro" : "scuro";
      themeBtn.textContent = `Tema: ${label}`;
    }
  }

  // Imposta subito il tema all'avvio (appena lo script viene caricato)
  setTheme(getTheme());

  // Click sul bottone: alterna light/dark
  if (themeBtn) {
    themeBtn.addEventListener("click", () => {
      const current =
        document.body.getAttribute("data-theme") === "light" ? "light" : "dark";
      setTheme(current === "light" ? "dark" : "light");
    });
  }

  // Dropdown "tradizionali":
  // - un <button class="dropdown-toggle"> controlla un <ul class="dropdown-panel" hidden>
  // - aria-controls collega bottone -> pannello
  // - aria-expanded indica lo stato (utile per accessibilità)
  const dropdownToggles = Array.from(
    document.querySelectorAll(".dropdown-toggle"),
  );

  function getControlledPanel(toggleBtn) {
    const panelId = toggleBtn.getAttribute("aria-controls");
    if (!panelId) return null;
    return document.getElementById(panelId);
  }

  function closeAllDropdowns(exceptToggle) {
    dropdownToggles.forEach((btn) => {
      if (exceptToggle && btn === exceptToggle) return;
      const panel = getControlledPanel(btn);
      btn.setAttribute("aria-expanded", "false");
      if (panel) panel.hidden = true;
    });
  }

  dropdownToggles.forEach((btn) => {
    btn.addEventListener("click", (event) => {
      event.preventDefault();

      const panel = getControlledPanel(btn);
      if (!panel) return;

      const isOpen = btn.getAttribute("aria-expanded") === "true";

      // Se lo sto aprendo, prima chiudo gli altri.
      if (!isOpen) closeAllDropdowns(btn);

      // Toggle stato corrente
      btn.setAttribute("aria-expanded", isOpen ? "false" : "true");
      panel.hidden = isOpen;
    });
  });

  // Quando clicchi un link della nav:
  // - richiudiamo il menu hamburger (checkbox unchecked)
  // - chiudiamo anche tutti i dropdown eventualmente aperti
  document.querySelectorAll(".nav a").forEach((link) => {
    link.addEventListener("click", () => {
      if (navToggle) navToggle.checked = false;
      closeAllDropdowns();
    });
  });

  // Click fuori dalla navbar: chiude i dropdown aperti
  const navEl = document.querySelector(".nav");
  document.addEventListener("click", (event) => {
    if (!navEl) return;
    const target = event.target;
    if (!(target instanceof Element)) return;
    if (navEl.contains(target)) return;
    closeAllDropdowns();
  });
})();
