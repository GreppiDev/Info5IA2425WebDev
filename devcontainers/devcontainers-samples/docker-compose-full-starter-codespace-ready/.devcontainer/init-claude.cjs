#!/usr/bin/env node
/**
 * Script per inizializzare Claude Code nel dev container.
 * - Imposta hasCompletedOnboarding: true in ~/.claude.json
 */

const fs = require("fs");
const path = require("path");
const os = require("os");

console.log("Inizializzazione Claude Code...");

const homeDir = os.homedir();
const claudeJsonPath = path.join(homeDir, ".claude.json");

try {
  let config = { hasCompletedOnboarding: true };

  if (fs.existsSync(claudeJsonPath)) {
    const existingContent = JSON.parse(
      fs.readFileSync(claudeJsonPath, "utf-8"),
    );
    config = { ...existingContent, hasCompletedOnboarding: true };
  }

  fs.writeFileSync(claudeJsonPath, JSON.stringify(config, null, 2), "utf-8");
  console.log("✓ Onboarding completato");
} catch (error) {
  console.error("✗ Errore durante l'onboarding:", error.message);
  process.exit(1);
}

console.log(
  "✓ Claude Code onboarding completato (configurazione da fare manualmente in ~/.claude/settings.json)",
);
