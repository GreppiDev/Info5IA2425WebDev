// File: wwwroot/js/template-loader.js

class TemplateLoader {
  /**
   * Carica un template HTML da un percorso specificato e lo inserisce
   * nell'elemento con l'ID fornito.
   * @param {string} elementId L'ID dell'elemento contenitore.
   * @param {string} templatePath Il percorso del file HTML del template.
   * @returns {Promise<void>} Una promise che si risolve quando il template è caricato o fallisce.
   */
  static async loadTemplate(elementId, templatePath) {
    const container = document.getElementById(elementId);
    if (!container) {
      console.warn(`TemplateLoader: Element with ID '${elementId}' not found.`);
      return Promise.resolve(); // Risolve comunque per non bloccare Promise.all
    }
    try {
      const response = await fetch(templatePath);
      if (!response.ok) {
        throw new Error(
          `HTTP error! status: ${response.status} for ${templatePath}`
        );
      }
      const content = await response.text();
      container.innerHTML = content; // Inserisce l'HTML caricato
      console.log(`Template '${templatePath}' loaded into #${elementId}`);
    } catch (error) {
      console.error(`Error loading template ${templatePath}:`, error);
      container.innerHTML = `<p class="text-danger text-center small">Error loading component: ${templatePath}</p>`; // Mostra errore nel contenitore
      // Rilancia l'errore per segnalare il fallimento a Promise.all
      throw error;
    }
  }

  /**
   * Inizializza e carica tutti i template comuni (navbar, footer).
   * Restituisce una Promise che si risolve quando tutti sono caricati o uno fallisce.
   * @returns {Promise<void>}
   */
  static async initializeCommonTemplates() {
    console.log("Initializing common templates...");
    // Restituisce la Promise risultante da Promise.all
    // Promise.all si risolve quando tutte le promise nell'array si risolvono,
    // o si rigetta non appena una delle promise si rigetta.
    return Promise.all([
      this.loadTemplate("navbar-container", "/components/navbar.html"),
      this.loadTemplate("footer-container", "/components/footer.html"),
      // Aggiungi qui altre chiamate per componenti comuni se necessario
    ])
      .then(() => {
        console.log("Common templates initialized successfully.");
      })
      .catch((error) => {
        console.error("Error initializing common templates:", error);
        // Rilancia l'errore per segnalare il fallimento all'await chiamante
        throw error;
      });
  }
}
