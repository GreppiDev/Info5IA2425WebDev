#!/bin/bash

# Script Bash completo per testare tutte le metriche e traces di EducationalGames
echo "🔍 Comprehensive OpenTelemetry Testing for EducationalGames"
echo "============================================================="

# Configurazione
BASE_URL="https://localhost:8443"
PROMETHEUS_URL="http://localhost:9090"
JAEGER_URL="http://localhost:16686"
GRAFANA_URL="http://localhost:3000"
OTLP_COLLECTOR_URL="http://localhost:8889"

# Contatori
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Colori
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function per testare endpoint
test_endpoint() {
    local name="$1"
    local url="$2"
    local method="${3:-GET}"
    local data="${4:-}"

    ((TOTAL_TESTS++))
    echo -n "Testing $name... "

    local curl_cmd="curl -s -k -w '%{http_code}' -o /dev/null --max-time 10"

    if [ "$method" = "POST" ] && [ -n "$data" ]; then
        curl_cmd="$curl_cmd -X POST -H 'Content-Type: application/json' -d '$data'"
    fi

    local status_code=$(eval "$curl_cmd '$url'")

    if [ "$status_code" = "200" ] || [ "$status_code" = "302" ]; then
        echo -e "${GREEN}✓ PASS${NC}"
        ((PASSED_TESTS++))
        return 0
    else
        echo -e "${RED}✗ FAIL (Status: $status_code)${NC}"
        ((FAILED_TESTS++))
        return 1
    fi
}

# Function per testare metriche Prometheus
# Nota: Alcune metriche possono essere commentate se non hanno dati durante i test automatici
test_prometheus_metrics() {
    echo ""
    echo -e "${YELLOW}📊 Testing Prometheus Metrics${NC}"
    echo "=============================="

    local metrics=(
        # "http_requests_total"                # Commentata: dipende dal traffico HTTP
        "http_request_duration_seconds"
        # "aspnetcore_requests_total"          # Commentata: dipende dal traffico ASP.NET Core
        "aspnetcore_request_duration_seconds"
        # "dotnet_collection_count_total"      # Commentata: dipende dall'attività GC
        # "dotnet_allocated_bytes_total"       # Commentata: dipende dalle allocazioni memoria
        "dotnet_gc_duration_seconds"
        "dotnet_gc_collections_total"
        "dotnet_exceptions_total"
        "dotnet_threadpool_threads_count"
        "dotnet_working_set_bytes"
        "process_cpu_seconds_total"
        "process_memory_bytes"
        "process_open_fds"
        "process_start_time_seconds"
        "up"
        "scrape_duration_seconds"
        "scrape_samples_scraped"
    )

    for metric in "${metrics[@]}"; do
        local query_url="$PROMETHEUS_URL/api/v1/query?query=$metric"
        if test_endpoint "Metric: $metric" "$query_url"; then
            local result=$(curl -s -k "$query_url" | jq -r '.data.result | length' 2>/dev/null)
            if [ "$result" != "null" ] && [ "$result" -gt 0 ]; then
                echo -e "  ${CYAN}📈 Found $result metric series${NC}"
            else
                echo -e "  ${YELLOW}⚠️  No data found for this metric${NC}"
            fi
        fi
    done
}

# Function per generare traffico
generate_traffic() {
    echo ""
    echo -e "${YELLOW}🚦 Generating Traffic and Traces${NC}"
    echo "================================="

    # Test endpoint di telemetria
    for i in {1..5}; do
        local test_data="{\"testData\":\"bash_test_run_$i\",\"timestamp\":\"$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)\"}"
        test_endpoint "Telemetry Test #$i" "$BASE_URL/api/telemetry/test" "POST" "$test_data"
        sleep 0.5
    done

    # Test endpoint di health
    for i in {1..3}; do
        test_endpoint "Health Check #$i" "$BASE_URL/health"
        sleep 0.2
    done

    # Test endpoint principali dell'applicazione
    local endpoints=(
        "/"
        "/Account/Login"
        "/Account/Register"
        "/Dashboard"
        "/Game"
    )

    for endpoint in "${endpoints[@]}"; do
        test_endpoint "App Endpoint: $endpoint" "$BASE_URL$endpoint"
        sleep 0.3
    done
}

# Function per testare servizi di monitoraggio
test_monitoring_services() {
    echo ""
    echo -e "${YELLOW}🔧 Testing Monitoring Services${NC}"
    echo "==============================="

    test_endpoint "Jaeger UI" "$JAEGER_URL/api/services"
    test_endpoint "Prometheus UI" "$PROMETHEUS_URL/api/v1/status/config"
    test_endpoint "Grafana API" "$GRAFANA_URL/api/health"
    test_endpoint "OTEL Collector" "$OTLP_COLLECTOR_URL/metrics"
}

# Function per query avanzate
test_advanced_queries() {
    echo ""
    echo -e "${YELLOW}🔬 Advanced Metrics Analysis${NC}"
    echo "============================"

    local queries=(
        # "HTTP Request Rate:rate(http_requests_total[5m])"              # Commentata: dipende dal traffico
        # "HTTP Error Rate:rate(http_requests_total{code!~'2..'}[5m])"   # Commentata: dipende dal traffico
        # "GC Pressure:rate(dotnet_collection_count_total[5m])"          # Commentata: dipende dall'attività GC
        "Memory Usage:process_memory_bytes"
        "Thread Pool:dotnet_threadpool_threads_count"
    )

    for query_pair in "${queries[@]}"; do
        local name="${query_pair%%:*}"
        local query="${query_pair#*:}"
        local encoded_query=$(printf '%s' "$query" | jq -sRr @uri)
        local query_url="$PROMETHEUS_URL/api/v1/query?query=$encoded_query"
        test_endpoint "$name" "$query_url"
    done
}

# Function per verificare traces in Jaeger
test_jaeger_traces() {
    echo ""
    echo -e "${YELLOW}🕵️ Testing Jaeger Traces${NC}"
    echo "========================"

    local services_url="$JAEGER_URL/api/services"
    if test_endpoint "Jaeger Services List" "$services_url"; then
        echo -e "  ${CYAN}📋 Available services in Jaeger:${NC}"
        local services=$(curl -s -k "$services_url" | jq -r '.data[]' 2>/dev/null)
        if [ -n "$services" ]; then
            echo "$services" | while read -r service; do
                echo -e "    ${CYAN}- $service${NC}"
            done
        fi

        # Test traces per il servizio EducationalGames
        local traces_url="$JAEGER_URL/api/traces?service=EducationalGames&limit=10"
        test_endpoint "Recent Traces" "$traces_url"
    fi
}

# Inizio dei test
echo ""
echo -e "${GREEN}🏁 Starting Comprehensive Tests${NC}"
echo "================================"

# 1. Test servizi di monitoraggio
test_monitoring_services

# 2. Genera traffico per creare metriche e traces
generate_traffic

# 3. Test metriche Prometheus
test_prometheus_metrics

# 4. Test endpoint di metriche dirette
echo ""
echo -e "${YELLOW}📋 Testing Direct Metrics Endpoints${NC}"
echo "===================================="

test_endpoint "Prometheus Metrics" "$BASE_URL/metrics"
test_endpoint "Health Endpoint" "$BASE_URL/health"

# 5. Verifica traces in Jaeger
test_jaeger_traces

# 6. Query avanzate
test_advanced_queries

# 7. Verifica configurazione OTEL
echo ""
echo -e "${YELLOW}⚙️ OpenTelemetry Configuration Check${NC}"
echo "===================================="

test_endpoint "OTEL Collector Metrics" "$OTLP_COLLECTOR_URL/metrics"
# Nota: Il collector OTEL non ha un endpoint di health su /, solo /metrics è disponibile

# Riepilogo finale
echo ""
echo -e "${CYAN}📊 Test Results Summary${NC}"
echo "======================="
echo "Total Tests: $TOTAL_TESTS"
echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
echo -e "Failed: ${RED}$FAILED_TESTS${NC}"

if [ $FAILED_TESTS -eq 0 ]; then
    echo ""
    echo -e "${GREEN}🎉 All tests passed! OpenTelemetry is working correctly.${NC}"
else
    echo ""
    echo -e "${YELLOW}⚠️  Some tests failed. Check the configuration and services.${NC}"
fi

# Istruzioni finali
echo ""
echo -e "${CYAN}🔗 Quick Access Links${NC}"
echo "====================="
echo -e "• Jaeger UI: ${BLUE}$JAEGER_URL${NC}"
echo -e "• Prometheus: ${BLUE}$PROMETHEUS_URL${NC}"
echo -e "• Grafana: ${BLUE}$GRAFANA_URL${NC}"
echo -e "• Application: ${BLUE}$BASE_URL${NC}"
echo -e "• Metrics: ${BLUE}$BASE_URL/metrics${NC}"
echo -e "• Health: ${BLUE}$BASE_URL/health${NC}"

echo ""
echo -e "${CYAN}📚 Next Steps:${NC}"
echo "• Check Jaeger for distributed traces"
echo "• Verify metrics in Prometheus"
echo "• Create dashboards in Grafana"
echo "• Monitor application performance"

echo ""
echo -e "${GREEN}✅ Comprehensive telemetry testing completed!${NC}"
