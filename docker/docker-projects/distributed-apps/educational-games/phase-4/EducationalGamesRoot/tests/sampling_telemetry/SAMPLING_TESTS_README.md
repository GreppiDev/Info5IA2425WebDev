# Test di Sampling OpenTelemetry

Questa cartella contiene diversi script per testare la strategia di sampling ibrida implementata nel progetto EducationalGames.

## Script Disponibili

### 📁 Script Base

| File | Descrizione | Piattaforma |
|------|-------------|-------------|
| `test-sampling.ps1` | Test base PowerShell (20 richieste) | Windows |
| `test-sampling.sh` | Test base Bash (20 richieste) | Linux/macOS |

### 📁 Script k6 (Avanzati)

| File | Descrizione | Caratteristiche |
|------|-------------|-----------------|
| `test-sampling.js` | Test base k6 | Equivalente agli script base ma con metriche dettagliate |
| `test-sampling-advanced.js` | Test avanzato k6 | Include trigger per tail sampling |
| `run-sampling-tests.sh` | Wrapper interattivo Bash | Menu per scegliere il tipo di test (Linux/macOS) |
| `run-sampling-tests.ps1` | Wrapper interattivo PowerShell | Menu per scegliere il tipo di test (Windows) |

## Come Usare

### Prerequisiti

1. **Applicazione in esecuzione**:
2. 
   ```bash
   docker-compose up -d
   ```

3. **Per script k6** (opzionale):
   - Installa k6: https://k6.io/docs/getting-started/installation/

### Esecuzione

#### Opzione 1: Script Semplici

```powershell
# Windows
.\test-sampling.ps1
```

```bash
# Linux/macOS
./test-sampling.sh
```

#### Opzione 2: Script k6 Avanzati

```powershell
# Windows - Test interattivo (raccomandato)
.\run-sampling-tests.ps1
```

```bash
# Linux/macOS - Test interattivo (raccomandato)
./run-sampling-tests.sh
```

```bash
# Test specifici (tutte le piattaforme)
k6 run test-sampling.js                 # Test base
k6 run test-sampling-advanced.js       # Test con trigger
```

## Cosa Aspettarsi

### Head Sampling (1%)

- **Test base**: Su 20 richieste normali, solo ~0-1 dovrebbe essere visibile in Jaeger
- **Beneficio**: Riduce drasticamente l'overhead di produzione

### Tail Sampling (Intelligente)

- **Alta latenza**: Tracce con durata > 500ms vengono sempre mantenute
- **Errori**: Tracce con errori HTTP (4xx, 5xx) vengono sempre mantenute
- **Beneficio**: Mantiene visibilità sui problemi reali

### Verifica Risultati

1. **Jaeger UI**: http://localhost:16686
   - Seleziona servizio "EducationalGames"
   - Cerca tracce negli ultimi 15 minuti
   - Nota la differenza tra tracce normali (poche) e tracce con problemi (tutte)

2. **Script k6**: Genera file di riepilogo JSON con metriche dettagliate

## Configurazione Sampling

Modifica i parametri nel file `.env`:

```bash
# Percentuale di tracce normali campionate (1% = produzione)
OTEL_TAIL_SAMPLING_PROBABILISTIC_PERCENT=1

# Soglia di latenza per tail sampling (500ms)
OTEL_TAIL_SAMPLING_LATENCY_THRESHOLD_MS=500
```

Dopo la modifica:

```bash
docker-compose restart otel-collector
```

## Script Avanzato k6: Dettagli

Il test avanzato (`test-sampling-advanced.js`) include:

- **Carico normale**: Richieste continue per simulare traffico reale
- **Test alta latenza**: Richieste all'endpoint `/api/telemetry/test` per simulare operazioni lente
- **Test errori**: Richieste a endpoint inesistenti per generare errori 404
- **Metriche**: Contatori per ogni tipo di richiesta e trigger di tail sampling

## Troubleshooting

### Applicazione non raggiungibile

```bash
# Verifica stato container
docker-compose ps

# Verifica logs
docker-compose logs webapp
```

### k6 non installato

```bash
# Ubuntu/Debian
sudo apt-get update && sudo apt-get install k6

# macOS
brew install k6

# Windows
winget install k6 --source winget 
```

### Script bash non eseguibili

```bash
# Linux/macOS
chmod +x *.sh
```
