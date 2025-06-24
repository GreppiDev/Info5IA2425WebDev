# Script PowerShell completo per testare tutte le metriche e traces di EducationalGames
Write-Host "🔍 Comprehensive OpenTelemetry Testing for EducationalGames" -ForegroundColor Cyan
Write-Host "=============================================================" -ForegroundColor Cyan

# Configurazione
$baseUrl = "https://localhost:8443"
$prometheusUrl = "http://localhost:9090"
$jaegerUrl = "http://localhost:16686"
$grafanaUrl = "http://localhost:3000"
$otlpCollectorUrl = "http://localhost:8889"

# Contatori per i risultati (globali)
$global:totalTests = 0
$global:passedTests = 0
$global:failedTests = 0

# Function per testare endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Body = @{},
        [string]$ContentType = "application/json",
        [bool]$SkipCertCheck = $true
    )
    
    $global:totalTests++
    Write-Host "Testing $Name... " -NoNewline
    
    try {
        $params = @{
            Uri             = $Url
            Method          = $Method
            TimeoutSec      = 10
            UseBasicParsing = $true
        }
        
        if ($SkipCertCheck) {
            $params.SkipCertificateCheck = $true
        }
        
        if ($Method -eq "POST" -and $Body.Count -gt 0) {
            $params.Body = ($Body | ConvertTo-Json)
            $params.ContentType = $ContentType
        }
        
        $response = Invoke-WebRequest @params
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ PASS" -ForegroundColor Green
            $global:passedTests++
            return $response
        }
        else {
            Write-Host "✗ FAIL (Status: $($response.StatusCode))" -ForegroundColor Red
            $global:failedTests++
            return $null
        }
    }
    catch {
        Write-Host "✗ FAIL ($($_.Exception.Message))" -ForegroundColor Red
        $global:failedTests++
        return $null
    }
}

# Function per testare metriche Prometheus
function Test-PrometheusMetrics {
    param([string[]]$MetricNames)
    
    Write-Host ""
    Write-Host "📊 Testing Prometheus Metrics" -ForegroundColor Yellow
    Write-Host "=============================="
    
    foreach ($metric in $MetricNames) {
        $queryUrl = "$prometheusUrl/api/v1/query?query=$metric"
        $response = Test-Endpoint "Metric: $metric" $queryUrl
        
        if ($response) {
            $data = $response.Content | ConvertFrom-Json
            if ($data.data.result.Count -gt 0) {
                Write-Host "  📈 Found $($data.data.result.Count) metric series" -ForegroundColor Gray
            }
            else {
                Write-Host "  ⚠️  No data found for this metric" -ForegroundColor Yellow
            }
        }
    }
}

# Function per generare traffico e traces
function Generate-TrafficAndTraces {
    Write-Host ""
    Write-Host "🚦 Generating Traffic and Traces" -ForegroundColor Yellow
    Write-Host "================================="
    
    # Test endpoint di telemetria
    for ($i = 1; $i -le 5; $i++) {
        $testData = @{
            testData  = "test_run_$i"
            timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        }
        Test-Endpoint "Telemetry Test #$i" "$baseUrl/api/telemetry/test" "POST" $testData
        Start-Sleep -Milliseconds 500
    }
    
    # Test endpoint di health
    for ($i = 1; $i -le 3; $i++) {
        Test-Endpoint "Health Check #$i" "$baseUrl/health"
        Start-Sleep -Milliseconds 200
    }
    
    # Test endpoint principali dell'applicazione
    $endpoints = @(
        "/",
        "/Account/Login",
        "/Account/Register",
        "/Dashboard",
        "/Game"
    )
    
    foreach ($endpoint in $endpoints) {
        Test-Endpoint "App Endpoint: $endpoint" "$baseUrl$endpoint"
        Start-Sleep -Milliseconds 300
    }
}

# Function per testare servizi di monitoraggio
function Test-MonitoringServices {
    Write-Host ""
    Write-Host "🔧 Testing Monitoring Services" -ForegroundColor Yellow
    Write-Host "==============================="
    
    Test-Endpoint "Jaeger UI" "$jaegerUrl/api/services"
    Test-Endpoint "Prometheus UI" "$prometheusUrl/api/v1/status/config"
    Test-Endpoint "Grafana API" "$grafanaUrl/api/health"
    Test-Endpoint "OTEL Collector" "$otlpCollectorUrl/metrics"
}

# Inizio dei test
Write-Host ""
Write-Host "🏁 Starting Comprehensive Tests" -ForegroundColor Green
Write-Host "================================"

# 1. Test servizi di monitoraggio
Test-MonitoringServices

# 2. Genera traffico per creare metriche e traces
Generate-TrafficAndTraces

# 3. Test metriche Prometheus specifiche
$metricsToTest = @(
    # Metriche ASP.NET Core
    "http_requests_total",
    "http_request_duration_seconds",
    "aspnetcore_requests_total",
    "aspnetcore_request_duration_seconds",
    
    # Metriche Runtime .NET
    "dotnet_collection_count_total",
    "dotnet_allocated_bytes_total",
    "dotnet_gc_duration_seconds",
    "dotnet_gc_collections_total",
    "dotnet_exceptions_total",
    "dotnet_threadpool_threads_count",
    "dotnet_working_set_bytes",
    
    # Metriche di sistema
    "process_cpu_seconds_total",
    "process_memory_bytes",
    "process_open_fds",
    "process_start_time_seconds",
    
    # Metriche personalizzate (se presenti)
    "up",
    "scrape_duration_seconds",
    "scrape_samples_scraped"
)

Test-PrometheusMetrics $metricsToTest

# 4. Test endpoint di metriche dirette
Write-Host ""
Write-Host "📋 Testing Direct Metrics Endpoints" -ForegroundColor Yellow
Write-Host "===================================="

Test-Endpoint "Prometheus Metrics" "$baseUrl/metrics"
Test-Endpoint "Health Endpoint" "$baseUrl/health"

# 5. Verifica traces in Jaeger
Write-Host ""
Write-Host "🕵️ Testing Jaeger Traces" -ForegroundColor Yellow
Write-Host "========================"

$jaegerServicesUrl = "$jaegerUrl/api/services"
$servicesResponse = Test-Endpoint "Jaeger Services List" $jaegerServicesUrl

if ($servicesResponse) {
    $services = $servicesResponse.Content | ConvertFrom-Json
    Write-Host "  📋 Available services in Jaeger:" -ForegroundColor Gray
    foreach ($service in $services.data) {
        Write-Host "    - $service" -ForegroundColor Gray
    }
    
    # Test traces per il servizio EducationalGames
    $tracesUrl = "$jaegerUrl/api/traces?service=EducationalGames&limit=10"
    Test-Endpoint "Recent Traces" $tracesUrl
}

# 6. Test avanzati delle metriche
Write-Host ""
Write-Host "🔬 Advanced Metrics Analysis" -ForegroundColor Yellow
Write-Host "============================"

# Query complesse per analisi
$advancedQueries = @(
    @{Name = "HTTP Request Rate"; Query = "rate(http_requests_total[5m])" },
    @{Name = "HTTP Error Rate"; Query = "rate(http_requests_total{code!~'2..'}[5m])" },
    @{Name = "GC Pressure"; Query = "rate(dotnet_collection_count_total[5m])" },
    @{Name = "Memory Usage"; Query = "process_memory_bytes" },
    @{Name = "Thread Pool"; Query = "dotnet_threadpool_threads_count" },
    @{Name = "Request Duration P95"; Query = "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))" },
    @{Name = "Request Duration P99"; Query = "histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[5m]))" }
)

foreach ($query in $advancedQueries) {
    $encodedQuery = [System.Web.HttpUtility]::UrlEncode($query.Query)
    $queryUrl = "$prometheusUrl/api/v1/query?query=$encodedQuery"
    Test-Endpoint $query.Name $queryUrl
}

# 7. Verifica configurazione OTEL
Write-Host ""
Write-Host "⚙️ OpenTelemetry Configuration Check" -ForegroundColor Yellow
Write-Host "===================================="

# Test endpoint del collector OTEL
Test-Endpoint "OTEL Collector Metrics" "$otlpCollectorUrl/metrics"
# Nota: Il collector OTEL non ha un endpoint di health su /, solo /metrics è disponibile

# Riepilogo finale
Write-Host ""
Write-Host "📊 Test Results Summary" -ForegroundColor Cyan
Write-Host "======================="
Write-Host "Total Tests: $global:totalTests" -ForegroundColor White
Write-Host "Passed: $global:passedTests" -ForegroundColor Green
Write-Host "Failed: $global:failedTests" -ForegroundColor Red

if ($global:failedTests -eq 0) {
    Write-Host ""
    Write-Host "🎉 All tests passed! OpenTelemetry is working correctly." -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "⚠️  Some tests failed. Check the configuration and services." -ForegroundColor Yellow
}

# Istruzioni finali
Write-Host ""
Write-Host "🔗 Quick Access Links" -ForegroundColor Cyan
Write-Host "====================="
Write-Host "• Jaeger UI: " -NoNewline
Write-Host "$jaegerUrl" -ForegroundColor Blue
Write-Host "• Prometheus: " -NoNewline
Write-Host "$prometheusUrl" -ForegroundColor Blue
Write-Host "• Grafana: " -NoNewline
Write-Host "$grafanaUrl" -ForegroundColor Blue
Write-Host "• Application: " -NoNewline
Write-Host "$baseUrl" -ForegroundColor Blue
Write-Host "• Metrics: " -NoNewline
Write-Host "$baseUrl/metrics" -ForegroundColor Blue
Write-Host "• Health: " -NoNewline
Write-Host "$baseUrl/health" -ForegroundColor Blue

Write-Host ""
Write-Host "📚 Next Steps:" -ForegroundColor Cyan
Write-Host "• Check Jaeger for distributed traces"
Write-Host "• Verify metrics in Prometheus"
Write-Host "• Create dashboards in Grafana"
Write-Host "• Monitor application performance"

Write-Host ""
Write-Host "✅ Comprehensive telemetry testing completed!" -ForegroundColor Green
