# Script wrapper per i test di sampling con k6 (versione PowerShell)
Write-Host "=== Test di Sampling OpenTelemetry ===" -ForegroundColor Green
Write-Host ""

# Verifica se k6 è installato
$k6Command = $null
if (Get-Command "k6" -ErrorAction SilentlyContinue) {
    $k6Command = "k6"
}
elseif (Get-Command "k6.exe" -ErrorAction SilentlyContinue) {
    $k6Command = "k6.exe"
}
else {
    Write-Host "Errore: k6 non è installato." -ForegroundColor Red
    Write-Host "Installa k6 da: https://k6.io/docs/getting-started/installation/" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Windows:" -ForegroundColor Cyan
    Write-Host "  # Chocolatey"
    Write-Host "  choco install k6"
    Write-Host ""
    Write-Host "  # Scoop"
    Write-Host "  scoop install k6"
    Write-Host ""
    Write-Host "  # Download diretto"
    Write-Host "  # Vai su https://github.com/grafana/k6/releases"
    Write-Host "  # Scarica k6-vX.X.X-windows-amd64.zip"
    Write-Host "  # Estrai k6.exe e aggiungi alla PATH"
    exit 1
}

Write-Host "✓ k6 trovato: $k6Command" -ForegroundColor Green

# Funzione per verificare se l'applicazione è in esecuzione
function Test-Application {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8080/" -Method GET -TimeoutSec 3 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Verifica se l'applicazione è in esecuzione
Write-Host "Verifica che l'applicazione sia in esecuzione..." -ForegroundColor Yellow
if (!(Test-Application)) {
    Write-Host "Errore: L'applicazione non è raggiungibile su http://localhost:8080/" -ForegroundColor Red
    Write-Host "Avvia l'applicazione con: docker-compose up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Applicazione in esecuzione" -ForegroundColor Green
Write-Host ""

# Menu di selezione
Write-Host "Seleziona il tipo di test:" -ForegroundColor Cyan
Write-Host "1) Test base sampling (equivalente allo script PowerShell)"
Write-Host "2) Test avanzato sampling (con trigger di tail sampling)"
Write-Host "3) Esegui entrambi i test"
Write-Host ""
$choice = Read-Host "Scegli un'opzione (1-3)"

switch ($choice) {
    "1" {
        Write-Host "Esecuzione test base sampling..." -ForegroundColor Yellow
        & $k6Command run test-sampling.js
    }
    "2" {
        Write-Host "Esecuzione test avanzato sampling..." -ForegroundColor Yellow
        & $k6Command run test-sampling-advanced.js
    }
    "3" {
        Write-Host "Esecuzione test base sampling..." -ForegroundColor Yellow
        & $k6Command run test-sampling.js
        Write-Host ""
        Write-Host "Esecuzione test avanzato sampling..." -ForegroundColor Yellow
        & $k6Command run test-sampling-advanced.js
    }
    default {
        Write-Host "Scelta non valida" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "=== Test completati! ===" -ForegroundColor Green
Write-Host "Controlla i risultati in:" -ForegroundColor Blue
Write-Host "  🔍 Jaeger UI: http://localhost:16686"
Write-Host "  📊 Prometheus: http://localhost:9090"
Write-Host "  📈 Grafana: http://localhost:3000"
Write-Host ""
Write-Host "Cosa aspettarsi:" -ForegroundColor Yellow
Write-Host "  • Head sampling al 1%: Solo ~1% delle richieste normali visibili"
Write-Host "  • Tail sampling: TUTTE le tracce con errori o latenza >500ms mantenute"
