class TemplateLoader {
  static async loadTemplate(elementId, templatePath) {
    try {
      const response = await fetch(templatePath);
      if (!response.ok)
        throw new Error(`HTTP error! status: ${response.status}`);
      const content = await response.text();
      document.getElementById(elementId).innerHTML = content;
    } catch (error) {
      console.error(`Error loading template ${templatePath}:`, error);
    }
  }

  static async initializeTemplates() {
    await Promise.all([
      this.loadTemplate("header-container", "/components/header.html"),
      this.loadTemplate("footer-container", "/components/footer.html"),
    ]);
  }
}
