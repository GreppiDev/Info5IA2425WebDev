# OpenTelemetry Implementation Guide

Questo progetto implementa un sistema completo di osservabilità utilizzando OpenTelemetry con Prometheus, Grafana e Jaeger, seguendo le best practices Microsoft.

## Architettura

```sh
EducationalGames App → OpenTelemetry Collector → Jaeger (Traces) + Prometheus (Metrics)
                                                                      ↓
                                                                  Grafana (Visualization)
```

## Componenti Implementati

### 1. **OpenTelemetry nella Applicazione (.NET)**

- **Tracing**: Tracciamento automatico di ASP.NET Core, HttpClient, Entity Framework Core
- **Metrics**: Metriche di runtime, ASP.NET Core, e metriche personalizzate
- **Export**: OTLP (OpenTelemetry Protocol) verso il Collector

### 2. **OpenTelemetry Collector**

- Riceve telemetria dall'applicazione via OTLP
- **Strategia di Sampling Ibrida per Produzione**:
  - **Head-based sampling**: 1% delle tracce (configurable)
  - **Tail-based sampling**: mantiene tracce con errori o alta latenza (>500ms)
- Configurazione dinamica tramite variabili d'ambiente
- Esporta traces verso Jaeger
- Esporta metriche verso Prometheus

### 3. **Jaeger** (Distributed Tracing)

- UI: `http://localhost:16686`
- Visualizzazione delle tracce distribuite
- Analisi delle performance e bottleneck

### 4. **Prometheus** (Metrics Collection)

- UI: `http://localhost:9090`
- Raccolta metriche da:
  - Applicazione (.NET)
  - OTel Collector
  - cAdvisor (Container metrics)
  - Node Exporter (System metrics)

### 5. **Grafana** (Visualization)

- UI: `http://localhost:3000`
- Dashboard per visualizzare metriche
- Integrazione con Prometheus come data source

## Configurazione

### Variabili d'Ambiente OpenTelemetry

```bash
OTEL_SERVICE_NAME=EducationalGames
OTEL_SERVICE_VERSION=1.0.0
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_RESOURCE_ATTRIBUTES=service.name=EducationalGames,service.version=1.0.0,deployment.environment=docker
OTEL_METRIC_EXPORT_INTERVAL=5000
```

### Configurazione Sampling Personalizzabile

Il sistema utilizza una strategia di sampling ibrida configurabile tramite variabili d'ambiente nel file `.env`:

```bash
# Parametri di sampling per produzione
OTEL_TAIL_SAMPLING_LATENCY_THRESHOLD_MS=500
OTEL_TAIL_SAMPLING_PROBABILISTIC_PERCENT=1
```

- **OTEL_TAIL_SAMPLING_LATENCY_THRESHOLD_MS**: Soglia di latenza sopra la quale tutte le tracce vengono mantenute (default: 500ms)
- **OTEL_TAIL_SAMPLING_PROBABILISTIC_PERCENT**: Percentuale di tracce campionate casualmente (default: 1%)

La configurazione viene applicata automaticamente tramite il template `otel-collector-config.yml.template`.

### Endpoints Importanti

- **Applicazione**: `https://localhost:8443` (HTTPS tramite Nginx)
- **Applicazione HTTP**: `http://localhost:8080` (HTTP, reindirizza a HTTPS)
- **Metriche Prometheus**: `https://localhost:8443/metrics`
- **Health Check**: `https://localhost:8443/health`
- **Test Telemetry**: `POST https://localhost:8443/api/telemetry/test`
- **Jaeger UI**: `http://localhost:16686`
- **Prometheus UI**: `http://localhost:9090`
- **Grafana UI**: `http://localhost:3000`

## Come Testare

### 1. Avviare l'ambiente

```bash
docker-compose up -d
```

### 2. Testare la telemetria

```bash
# Test dell'endpoint telemetry
curl -X POST "http://localhost:8080/api/telemetry/test" \
     -H "Content-Type: application/json" \
     -d '{"testData": "hello_world"}'
```

### 3. Verificare le tracce in Jaeger

1. Apri `http://localhost:16686`
2. Seleziona il servizio "EducationalGames"
3. Clicca "Find Traces"
4. Visualizza le tracce generate

### 4. Verificare le metriche in Prometheus

1. Apri `http://localhost:9090`
2. Cerca metriche come:
   - `http_requests_total`
   - `dotnet_*`
   - `process_*`

### 5. Configurare Grafana

1. Apri `http://localhost:3000`
2. Login: admin/admin
3. Aggiungi Prometheus come data source: `http://prometheus:9090`
4. Importa dashboard per .NET applications

### 6. Testare il Sampling

Sono disponibili diversi script per testare la strategia di sampling:

#### Script PowerShell (Windows)

```powershell
# Test base di sampling
.\test-sampling.ps1
```

#### Script Bash (Linux/macOS)

```bash
# Test base di sampling
./test-sampling.sh
```

#### Script k6 (Avanzato - Multipiattaforma)

```bash
# Test interattivo con menu
./run-sampling-tests.sh

# Oppure esegui direttamente:
k6 run test-sampling.js                 # Test base
k6 run test-sampling-advanced.js       # Test avanzato con trigger
```

**Cosa fanno i test:**

1. **Test base**: Genera 20 richieste rapide all'applicazione
2. **Test avanzato k6**: Include richieste ad alta latenza e con errori per testare il tail sampling
3. Con head sampling al 1%, dovresti vedere circa 0-1 tracce normali in Jaeger
4. Le tracce con errori o latenza > 500ms saranno comunque mantenute

**Modifica parametri di sampling:**

```bash
# Modifica il file .env:
# OTEL_TAIL_SAMPLING_PROBABILISTIC_PERCENT per cambiare la percentuale
# OTEL_TAIL_SAMPLING_LATENCY_THRESHOLD_MS per la soglia di latenza

# Poi riavvia i container:
docker-compose restart
```

## Metriche Personalizzate

Il servizio `TelemetryDemoService` dimostra come:

- Creare Activity personalizzate per il tracing
- Aggiungere tag e attributi alle tracce
- Gestire errori nel tracing
- Strutturare operazioni complesse in step

## Monitoraggio in Produzione

### Metriche Chiave da Monitorare:

- **Latenza**: Tempo di risposta delle richieste
- **Throughput**: Richieste per secondo
- **Errori**: Tasso di errore delle richieste
- **Saturazione**: Utilizzo CPU, memoria, connessioni DB

### Alert Raccomandati:

- Latenza > 2 secondi
- Tasso di errore > 5%
- Utilizzo CPU > 80%
- Utilizzo memoria > 90%

## Struttura File

```text
/
├── otel-collector-config.yml    # Configurazione OTel Collector
├── prometheus/prometheus.yml    # Configurazione Prometheus
├── .env                         # Variabili d'ambiente (incluse quelle OTel)
├── .env.example                 # Template delle variabili d'ambiente
├── docker-compose.yml           # Orchestrazione container
└── EducationalGames/
    ├── Program.cs               # Configurazione OTel in .NET
    └── Services/
        └── TelemetryDemoService.cs  # Esempio di tracing personalizzato
```

## Best Practices Implementate

1. **Sampling**: Configurato per bilanciare performance e visibilità
2. **Resource Attributes**: Metadati del servizio per identificazione
3. **Health Check**: Endpoint per monitoring dell'applicazione
4. **Structured Logging**: Log strutturati per correlazione con traces
5. **Error Handling**: Gestione appropriata degli errori nel tracing
6. **Performance**: Filtering delle richieste non critiche
