# Test script per verificare il tail-based sampling
Write-Host "Avvio test delle policy di sampling..." -ForegroundColor Green

# Genera 20 richieste rapide per testare il head sampling (1%)
Write-Host "Generazione di 20 richieste rapide per testare head sampling (1%)..." -ForegroundColor Yellow
for ($i = 1; $i -le 20; $i++) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8080/" -Method GET -TimeoutSec 5
        Write-Host "Richiesta $i completata" -ForegroundColor Cyan
        Start-Sleep -Milliseconds 100
    }
    catch {
        Write-Host "Richiesta $i fallita: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Test completato! Controlla Jaeger UI per vedere le tracce filtrate." -ForegroundColor Green
Write-Host "URL Jaeger: http://localhost:16686" -ForegroundColor Blue
Write-Host ""
Write-Host "Con head sampling al 1%, dovresti vedere circa 0-1 tracce dalle 20 richieste." -ForegroundColor Yellow
Write-Host "Le tracce con errori o latenza > 500ms saranno comunque mantenute dal tail sampling." -ForegroundColor Yellow
