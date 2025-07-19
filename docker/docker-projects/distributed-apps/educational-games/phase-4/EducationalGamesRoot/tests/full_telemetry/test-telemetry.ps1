# Script PowerShell per testare l'implementazione OpenTelemetry
Write-Host "🚀 Testing OpenTelemetry Implementation for EducationalGames" -ForegroundColor Cyan

# Function to check if service is running
function Test-Service {
    param(
        [string]$ServiceName,
        [string]$Url
    )
    
    Write-Host "Checking $ServiceName... " -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ Running" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "✗ Not accessible" -ForegroundColor Red
        return $false
    }
}

# Function to test telemetry endpoint
function Test-TelemetryEndpoint {
    Write-Host "Testing telemetry endpoint... " -NoNewline
    
    try {
        $body = @{
            testData = "hello_opentelemetry"
        } | ConvertTo-Json
        $response = Invoke-RestMethod -Uri "https://localhost:8443/api/telemetry/test" `
            -Method Post `
            -Body $body `
            -ContentType "application/json" `
            -TimeoutSec 10 `
            -SkipCertificateCheck
        
        if ($response.success -eq $true) {
            Write-Host "✓ Success" -ForegroundColor Green
            Write-Host "  Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "✗ Failed" -ForegroundColor Red
            Write-Host "  Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
            return $false
        }
    }
    catch {
        Write-Host "✗ Failed" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Gray
        return $false
    }
}

Write-Host ""
Write-Host "📋 Service Health Check" -ForegroundColor Yellow
Write-Host "======================"

# Check all services
Test-Service "Educational Games App" "https://localhost:8443/health"
Test-Service "Prometheus Metrics" "https://localhost:8443/metrics"
Test-Service "Jaeger UI" "http://localhost:16686"
Test-Service "Prometheus UI" "http://localhost:9090"
Test-Service "Grafana UI" "http://localhost:3000"
Test-Service "OTel Collector Metrics" "http://localhost:8889/metrics"

Write-Host ""
Write-Host "🧪 Telemetry Testing" -ForegroundColor Yellow
Write-Host "==================="

# Test telemetry endpoint multiple times to generate traces
for ($i = 1; $i -le 3; $i++) {
    Write-Host "Test run ${i}:"
    Test-TelemetryEndpoint
    Start-Sleep -Seconds 2
}

Write-Host ""
Write-Host "📊 Verification Steps" -ForegroundColor Yellow
Write-Host "===================="
Write-Host "1. " -NoNewline
Write-Host "Jaeger Traces" -ForegroundColor Magenta -NoNewline
Write-Host ": http://localhost:16686"
Write-Host "   - Select service: EducationalGames"
Write-Host "   - Click 'Find Traces' to see generated traces"
Write-Host ""
Write-Host "2. " -NoNewline
Write-Host "Prometheus Metrics" -ForegroundColor Magenta -NoNewline
Write-Host ": http://localhost:9090"
Write-Host "   - Query: http_requests_total"
Write-Host "   - Query: dotnet_collection_count_total"
Write-Host ""
Write-Host "3. " -NoNewline
Write-Host "Grafana Dashboards" -ForegroundColor Magenta -NoNewline
Write-Host ": http://localhost:3000"
Write-Host "   - Login: admin/admin"
Write-Host "   - Add Prometheus data source: http://prometheus:9090"
Write-Host ""
Write-Host "4. " -NoNewline
Write-Host "Application Metrics" -ForegroundColor Magenta -NoNewline
Write-Host ": https://localhost:8443/metrics"
Write-Host "   - Direct view of Prometheus metrics"
Write-Host ""

Write-Host "✅ OpenTelemetry testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📚 For more information, check OPENTELEMETRY_GUIDE.md" -ForegroundColor Cyan
