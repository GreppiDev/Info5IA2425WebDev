// JS base: seleziono elementi, ascolto eventi, aggiorno il DOM.

// Eseguo questo codice SOLO quando il DOM è pronto.
// DOMContentLoaded scatta quando il browser ha finito di caricare e costruire la struttura HTML (il DOM),
// quindi posso usare document.getElementById(...) senza rischiare che gli elementi non esistano ancora.
document.addEventListener("DOMContentLoaded", () => {
  // 1) Seleziono gli elementi dal DOM (con l'id definito in page.html)
  const greetForm = document.getElementById("greetForm");
  const nameInput = document.getElementById("nameInput");
  const message = document.getElementById("message");

  const countBtn = document.getElementById("countBtn");
  const countValue = document.getElementById("countValue");

  const themeBtn = document.getElementById("themeBtn");
  const timeValue = document.getElementById("timeValue");

  // 2) Stato interno dello script (una variabile che tiene il conteggio dei click)
  let clicks = 0;

  // 3) Funzione di utilità: aggiorna l'orario mostrato in pagina
  function updateTime() {
    const now = new Date();
    timeValue.textContent = now.toLocaleTimeString();
  }

  // Aggiorna ora subito e poi ogni secondo
  updateTime();
  setInterval(updateTime, 1000);

  // 4) Evento del form: intercetto l'invio per non ricaricare la pagina
  greetForm.addEventListener("submit", (event) => {
    // Impedisce il comportamento di default del form (reload della pagina)
    event.preventDefault();

    // Leggo il valore inserito, tolgo spazi a inizio/fine e aggiorno il testo nel DOM
    const name = (nameInput.value || "").trim();
    message.textContent = name
      ? `Ciao, ${name}!`
      : "Ciao! (Inserisci un nome per personalizzare il messaggio.)";

    // Stampo un messaggio in console (utile per vedere cosa succede in DevTools)
    console.log("Saluto inviato:", name || "(vuoto)");
  });

  // 5) Evento click: incrementa un contatore e aggiorna lo span in pagina
  countBtn.addEventListener("click", () => {
    clicks += 1;
    countValue.textContent = String(clicks);
  });

  // 6) Evento click: alterna tra tema chiaro e scuro cambiando un attributo sul body
  // Il CSS reagisce a body[data-theme="light"] cambiando le variabili dei colori.
  themeBtn.addEventListener("click", () => {
    const isLight = document.body.getAttribute("data-theme") === "light";
    document.body.setAttribute("data-theme", isLight ? "dark" : "light");
  });
});
