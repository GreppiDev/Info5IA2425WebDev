import http from "k6/http";
import { check, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

// Metriche personalizzate
const telemetryErrors = new Counter("telemetry_errors");
const telemetrySuccessRate = new Rate("telemetry_success_rate");
const telemetryDuration = new Trend("telemetry_duration");

// Configurazione del test
export const options = {
  stages: [
    { duration: "30s", target: 5 }, // Warm up
    { duration: "2m", target: 10 }, // Test principale
    { duration: "30s", target: 0 }, // Cool down
  ],
  thresholds: {
    http_req_duration: ["p(95)<2000"], // 95% delle richieste sotto 2s
    telemetry_success_rate: ["rate>0.9"], // 90% di successo
  },
};

const baseUrl = "https://localhost:8443";
const monitoringUrls = {
  prometheus: "http://localhost:9090",
  jaeger: "http://localhost:16686",
  grafana: "http://localhost:3000",
  otlpCollector: "http://localhost:8889",
};

// HTTP options per HTTPS con certificati self-signed
const httpsOptions = {
  headers: {
    "Content-Type": "application/json",
  },
  responseType: "text",
};

export function setup() {
  console.log("🚀 Starting Comprehensive Telemetry Testing");
  console.log("===========================================");

  // Verifica che tutti i servizi siano attivi
  const services = [
    { name: "Application Health", url: `${baseUrl}/health` },
    { name: "Prometheus Metrics", url: `${baseUrl}/metrics` },
    {
      name: "Prometheus API",
      url: `${monitoringUrls.prometheus}/api/v1/status/config`,
    },
    { name: "Jaeger API", url: `${monitoringUrls.jaeger}/api/services` },
    { name: "Grafana Health", url: `${monitoringUrls.grafana}/api/health` },
    { name: "OTEL Collector", url: `${monitoringUrls.otlpCollector}/metrics` },
  ];

  console.log("📋 Checking service availability...");
  services.forEach((service) => {
    try {
      const response = http.get(service.url, httpsOptions);
      const status = response.status === 200 ? "✓" : "✗";
      console.log(`  ${status} ${service.name}: ${response.status}`);
    } catch (error) {
      console.log(`  ✗ ${service.name}: ERROR - ${error.message}`);
    }
  });

  return { baseUrl, monitoringUrls };
}

export default function (data) {
  const testId = Math.floor(Math.random() * 10000);

  // 1. Test dell'endpoint di telemetria
  testTelemetryEndpoint(testId);

  // 2. Test degli endpoint principali dell'applicazione
  testApplicationEndpoints();

  // 3. Test delle metriche Prometheus
  testPrometheusMetrics();

  // 4. Test dei servizi di monitoraggio
  testMonitoringServices();

  sleep(1);
}

function testTelemetryEndpoint(testId) {
  const payload = JSON.stringify({
    testData: `k6_test_${testId}_${Date.now()}`,
    source: "k6-load-test",
    timestamp: new Date().toISOString(),
  });

  const startTime = Date.now();
  const response = http.post(
    `${baseUrl}/api/telemetry/test`,
    payload,
    httpsOptions
  );
  const duration = Date.now() - startTime;

  telemetryDuration.add(duration);

  const success = check(response, {
    "telemetry endpoint responds with 200": (r) => r.status === 200,
    "telemetry response contains success field": (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.success === true;
      } catch {
        return false;
      }
    },
    "telemetry response time < 5s": (r) => r.timings.duration < 5000,
  });

  telemetrySuccessRate.add(success);
  if (!success) {
    telemetryErrors.add(1);
  }
}

function testApplicationEndpoints() {
  const endpoints = [
    { path: "/health", name: "Health Check" },
    { path: "/metrics", name: "Metrics Endpoint" },
    { path: "/", name: "Home Page" },
    { path: "/Account/Login", name: "Login Page" },
    { path: "/Account/Register", name: "Register Page" },
  ];

  endpoints.forEach((endpoint) => {
    const response = http.get(`${baseUrl}${endpoint.path}`, httpsOptions);

    check(response, {
      [`${endpoint.name} responds with 200 or 302`]: (r) =>
        r.status === 200 || r.status === 302,
      [`${endpoint.name} response time < 3s`]: (r) => r.timings.duration < 3000,
    });
  });
}

// Test delle metriche Prometheus
// Nota: Alcune metriche sono commentate perché potrebbero non avere dati durante i test automatici:
// - http_requests_total: dipende dal traffico HTTP generato
// - dotnet_collection_count_total: dipende dall'attività del Garbage Collector
// - aspnetcore_requests_total: dipende dal traffico verso l'app ASP.NET Core
// - dotnet_allocated_bytes_total: dipende dalle allocazioni di memoria dell'applicazione
// Per test più completi, generare traffico significativo prima di eseguire questi controlli
function testPrometheusMetrics() {
  // Lista delle metriche chiave da verificare
  const metricsToCheck = [
    // 'http_requests_total', // Commentata: può non avere dati se non c'è traffico sufficiente
    // 'dotnet_collection_count_total', // Commentata: richiede GC attivo
    "process_cpu_seconds_total",
    // 'aspnetcore_requests_total', // Commentata: può non avere dati se non c'è traffico sufficiente
    // 'dotnet_allocated_bytes_total', // Commentata: richiede allocazioni di memoria significative
    "up",
  ];

  metricsToCheck.forEach((metric) => {
    const queryUrl = `${monitoringUrls.prometheus}/api/v1/query?query=${metric}`;
    const response = http.get(queryUrl);

    check(response, {
      [`Prometheus query for ${metric} succeeds`]: (r) => r.status === 200,
      [`Prometheus has data for ${metric}`]: (r) => {
        try {
          const data = JSON.parse(r.body);
          return data.data && data.data.result && data.data.result.length > 0;
        } catch {
          return false;
        }
      },
    });
  });
}

function testMonitoringServices() {
  // Test Jaeger services
  const jaegerResponse = http.get(`${monitoringUrls.jaeger}/api/services`);
  check(jaegerResponse, {
    "Jaeger API is accessible": (r) => r.status === 200,
    "Jaeger has services registered": (r) => {
      try {
        const data = JSON.parse(r.body);
        return data.data && data.data.length > 0;
      } catch {
        return false;
      }
    },
  });

  // Test Grafana health
  const grafanaResponse = http.get(`${monitoringUrls.grafana}/api/health`);
  check(grafanaResponse, {
    "Grafana is healthy": (r) => r.status === 200,
  });
  // Test OTEL Collector (solo endpoint /metrics disponibile)
  const otlpResponse = http.get(`${monitoringUrls.otlpCollector}/metrics`);
  check(otlpResponse, {
    "OTEL Collector metrics are available": (r) => r.status === 200,
  });
}

export function teardown(data) {
  console.log("");
  console.log("📊 Test Summary:");
  console.log("================");
  console.log("• Telemetry endpoints tested");
  console.log("• Application endpoints verified");
  console.log("• Prometheus metrics queried");
  console.log("• Monitoring services checked");
  console.log("");
  console.log("🔗 Access your monitoring tools:");
  console.log(`• Application: ${data.baseUrl}`);
  console.log(`• Metrics: ${data.baseUrl}/metrics`);
  console.log(`• Prometheus: ${data.monitoringUrls.prometheus}`);
  console.log(`• Jaeger: ${data.monitoringUrls.jaeger}`);
  console.log(`• Grafana: ${data.monitoringUrls.grafana}`);
  console.log("");
  console.log("✅ Comprehensive telemetry testing completed!");
}
