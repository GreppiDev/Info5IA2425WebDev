# 🎯 Soluzione Completa: Test di Sampling OpenTelemetry

## ✅ Script Implementati

La soluzione ora include script di test multipiattaforma per verificare la strategia di sampling ibrida:

### 📁 File Creati

| File | Descrizione | Piattaforma | Caratteristiche |
|------|-------------|-------------|-----------------|
| `test-sampling.ps1` | Script PowerShell originale | Windows | Test base (20 richieste) |
| `test-sampling.sh` | Equivalente bash | Linux/macOS | Test base (20 richieste) |
| `test-sampling.js` | Script k6 base | Multipiattaforma | Metriche dettagliate |
| `test-sampling-advanced.js` | Script k6 avanzato | Multipiattaforma | Trigger tail sampling |
| `run-sampling-tests.sh` | Wrapper interattivo Bash | Linux/macOS | Menu di selezione |
| `run-sampling-tests.ps1` | Wrapper interattivo PowerShell | Windows | Menu di selezione |
| `SAMPLING_TESTS_README.md` | Documentazione | Tutte | Guida completa |

## 🔧 Funzionalità Implementate

### Script Base (PowerShell/Bash/k6)

- **20 richieste HTTP** rapide all'applicazione
- **Intervallo di 100ms** tra le richieste
- **Logging colorato** per feedback visivo
- **Gestione errori** con messaggi informativi

### Script k6 Avanzato

- **Scenari multipli**:
  - Carico normale continuo
  - Test alta latenza (trigger tail sampling)
  - Test errori HTTP (trigger tail sampling)
- **Metriche personalizzate**:
  - Contatori per tipo di richiesta
  - Rate di trigger tail sampling
  - Durate e percentili
- **Report dettagliato** con istruzioni

### Wrapper Interattivo

- **Menu di selezione** per tipo di test
- **Verifica prerequisiti** (k6 installato, app running)
- **Esecuzione guidata** con feedback
- **Link diretti** alle UI di monitoraggio

## 🚀 Come Utilizzare

### Opzione 1: Script Semplici

```powershell
# Windows
.\test-sampling.ps1
```

```bash
# Linux/macOS  
./test-sampling.sh
```

### Opzione 2: k6 Base

```bash
k6 run test-sampling.js
```

### Opzione 3: k6 Avanzato

```bash
# Test completo con trigger
k6 run test-sampling-advanced.js
```

```powershell
# Test interattivo Windows
.\run-sampling-tests.ps1
```

```bash
# Test interattivo Linux/macOS
./run-sampling-tests.sh
```

## 📊 Risultati Attesi

### Head Sampling (1%)

- **Da 20 richieste normali**: solo ~0-1 traccia visibile in Jaeger
- **Beneficio**: Riduzione overhead del 99%

### Tail Sampling (Intelligente)

- **Alta latenza**: Tracce >500ms sempre mantenute
- **Errori HTTP**: Tracce 4xx/5xx sempre mantenute
- **Beneficio**: Visibilità completa sui problemi

### Verifica in Jaeger

1. Apri http://localhost:16686
2. Seleziona servizio "EducationalGames"
3. Cerca tracce negli ultimi 15 minuti
4. Osserva la differenza tra tracce normali (poche) e problematiche (tutte)

## 🎯 Vantaggi della Soluzione

### Multipiattaforma

- **Windows**: Script PowerShell nativo
- **Linux/macOS**: Script bash + wrapper interattivo
- **Universale**: Script k6 per tutte le piattaforme

### Livelli di Complessità

- **Principianti**: Script semplici equivalenti
- **Intermedio**: k6 base con metriche
- **Avanzato**: k6 con scenari multipli e trigger

### Testing Completo

- **Head sampling**: Verifica riduzione overhead
- **Tail sampling**: Verifica mantenimento tracce critiche
- **Scenari reali**: Simulazione di problemi produzione

### Documentazione

- **README dedicato**: Guida completa
- **Esempi pratici**: Comandi pronti all'uso
- **Troubleshooting**: Risoluzione problemi comuni

## 🔧 Configurazione Flessibile

I parametri di sampling sono configurabili nel file `.env`:

```bash
# Percentuale tracce normali (1% = produzione)
OTEL_TAIL_SAMPLING_PROBABILISTIC_PERCENT=1

# Soglia latenza tail sampling (500ms)
OTEL_TAIL_SAMPLING_LATENCY_THRESHOLD_MS=500
```

Modifica i valori e riavvia:

```bash
docker-compose restart otel-collector
```

## 🎉 Soluzione Completa

La suite di test fornisce ora:

- ✅ **Copertura multipiattaforma** completa
- ✅ **Livelli di complessità** graduali  
- ✅ **Test realistici** per produzione
- ✅ **Documentazione** esaustiva
- ✅ **Configurazione** flessibile
- ✅ **Feedback** dettagliato

Il sistema è pronto per essere utilizzato in ambienti didattici e produttivi! 🚀
