#!/usr/bin/env node
/**
 * Script per inizializzare Claude Code nel dev container
 * - Imposta hasCompletedOnboarding: true in ~/.claude.json
 * - Copia settings.json dalla configurazione montata
 */

const fs = require('fs');
const path = require('path');
const os = require('os');

// Imposta hasCompletedOnboarding in ~/.claude.json
const homeDir = os.homedir();
const claudeJsonPath = path.join(homeDir, '.claude.json');

console.log('Inizializzazione Claude Code...');

try {
  let config = { hasCompletedOnboarding: true };
  
  if (fs.existsSync(claudeJsonPath)) {
    const existingContent = JSON.parse(fs.readFileSync(claudeJsonPath, 'utf-8'));
    config = { ...existingContent, hasCompletedOnboarding: true };
  }
  
  fs.writeFileSync(claudeJsonPath, JSON.stringify(config, null, 2), 'utf-8');
  console.log('✓ Onboarding completato');
} catch (error) {
  console.error('✗ Errore durante l\'onboarding:', error.message);
  process.exit(1);
}

// Copia settings.json se esiste
const claudeDir = path.join(homeDir, '.claude');
const sourceSettings = '/mnt/claude-config/settings.json';
const targetSettings = path.join(claudeDir, 'settings.json');

try {
  if (!fs.existsSync(claudeDir)) {
    fs.mkdirSync(claudeDir, { recursive: true });
  }
  
  if (fs.existsSync(sourceSettings)) {
    fs.copyFileSync(sourceSettings, targetSettings);
    console.log('✓ Configurazione copiata in ~/.claude/settings.json');
  } else {
    console.warn('⚠ Warning: settings.json non trovato in .claude-config/');
  }
} catch (error) {
  console.error('✗ Errore durante la copia della configurazione:', error.message);
  process.exit(1);
}

console.log('✓ Claude Code configurato con successo!');

