#!/usr/bin/env node
/**
 * Script per inizializzare OpenCode nel dev container
 * - Copia auth.json nella posizione corretta
 */

const fs = require('fs');
const path = require('path');
const os = require('os');

const homeDir = os.homedir();
const opencodeDir = path.join(homeDir, '.local/share/opencode');
const sourceOpencodeAuth = '/mnt/opencode-config/auth.json';
const targetOpencodeAuth = path.join(opencodeDir, 'auth.json');

console.log('Inizializzazione OpenCode...');

try {
  if (!fs.existsSync(opencodeDir)) {
    fs.mkdirSync(opencodeDir, { recursive: true });
  }
  
  if (fs.existsSync(sourceOpencodeAuth)) {
    fs.copyFileSync(sourceOpencodeAuth, targetOpencodeAuth);
    console.log('✓ Autenticazione copiata in ~/.local/share/opencode/auth.json');
  } else {
    console.warn('⚠ Warning: auth.json non trovato in .opencode-config/');
  }
} catch (error) {
  console.error('✗ Errore durante la copia dell\'autenticazione:', error.message);
  process.exit(1);
}

console.log('✓ OpenCode configurato con successo!');
