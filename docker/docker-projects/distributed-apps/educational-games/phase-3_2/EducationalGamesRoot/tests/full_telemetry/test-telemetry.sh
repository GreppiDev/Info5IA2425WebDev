#!/bin/bash

# Script per testare l'implementazione OpenTelemetry
echo "🚀 Testing OpenTelemetry Implementation for EducationalGames"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if service is running
check_service() {
    local service_name=$1
    local url=$2
    echo -n "Checking $service_name... "

    if curl -s -k --max-time 5 "$url" >/dev/null; then
        echo -e "${GREEN}✓ Running${NC}"
        return 0
    else
        echo -e "${RED}✗ Not accessible${NC}"
        return 1
    fi
}

# Function to test telemetry endpoint
test_telemetry() {
    echo -n "Testing telemetry endpoint... "

    response=$(curl -s -X POST "https://localhost:8443/api/telemetry/test" \
        -H "Content-Type: application/json" \
        -d '{"testData": "hello_opentelemetry"}' \
        --max-time 10 \
        --insecure)

    if echo "$response" | grep -q "success.*true"; then
        echo -e "${GREEN}✓ Success${NC}"
        echo "  Response: $response"
        return 0
    else
        echo -e "${RED}✗ Failed${NC}"
        echo "  Response: $response"
        return 1
    fi
}

echo ""
echo "📋 Service Health Check"
echo "======================"

# Check all services
check_service "Educational Games App" "https://localhost:8443/health"
check_service "Prometheus Metrics" "https://localhost:8443/metrics"
check_service "Jaeger UI" "http://localhost:16686"
check_service "Prometheus UI" "http://localhost:9090"
check_service "Grafana UI" "http://localhost:3000"
check_service "OTel Collector Metrics" "http://localhost:8889/metrics"

echo ""
echo "🧪 Telemetry Testing"
echo "==================="

# Test telemetry endpoint multiple times to generate traces
for i in {1..3}; do
    echo "Test run $i:"
    test_telemetry
    sleep 2
done

echo ""
echo "📊 Verification Steps"
echo "===================="
echo "1. ${YELLOW}Jaeger Traces${NC}: http://localhost:16686"
echo "   - Select service: EducationalGames"
echo "   - Click 'Find Traces' to see generated traces"
echo ""
echo "2. ${YELLOW}Prometheus Metrics${NC}: http://localhost:9090"
echo "   - Query: http_requests_total"
echo "   - Query: dotnet_collection_count_total"
echo ""
echo "3. ${YELLOW}Grafana Dashboards${NC}: http://localhost:3000"
echo "   - Login: admin/admin"
echo "   - Add Prometheus data source: http://prometheus:9090"
echo ""
echo "4. ${YELLOW}Application Metrics${NC}: https://localhost:8443/metrics"
echo "   - Direct view of Prometheus metrics"
echo ""

echo "✅ OpenTelemetry testing completed!"
echo ""
echo "📚 For more information, check OPENTELEMETRY_GUIDE.md"
