import http from "k6/http";
import { check, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

// Metriche personalizzate
const highLatencyRequests = new Counter("high_latency_requests");
const normalRequests = new Counter("normal_requests");
const errorRequests = new Counter("error_requests");
const tailSamplingTriggers = new Rate("tail_sampling_triggers");

// Configurazione del test avanzato
export let options = {
  scenarios: {
    // Test normale per head sampling
    normal_load: {
      executor: "constant-vus",
      vus: 2,
      duration: "30s",
      tags: { test_type: "normal_load" },
    },
    // Test per triggeri di tail sampling (alta latenza)
    high_latency_test: {
      executor: "per-vu-iterations",
      vus: 1,
      iterations: 5,
      startTime: "10s",
      tags: { test_type: "high_latency" },
    },
    // Test per errori (triggeri di tail sampling)
    error_test: {
      executor: "per-vu-iterations",
      vus: 1,
      iterations: 3,
      startTime: "20s",
      tags: { test_type: "error_generation" },
    },
  },
  thresholds: {
    http_req_duration: ["p(95)<2000"],
    tail_sampling_triggers: ["rate>0"], // Dovremmo avere almeno qualche trigger
  },
};

export default function () {
  const scenario = __ENV.K6_SCENARIO || "normal_load";

  switch (scenario) {
    case "normal_load":
      performNormalRequest();
      break;
    case "high_latency":
      performHighLatencyRequest();
      break;
    case "error_generation":
      performErrorRequest();
      break;
    default:
      performNormalRequest();
  }
}

function performNormalRequest() {
  const response = http.get("http://localhost:8080/", {
    timeout: "5s",
    tags: { request_type: "normal" },
  });

  normalRequests.add(1);

  check(response, {
    "normal request status is 200": (r) => r.status === 200,
  });

  console.log(
    `Normal request - VU: ${__VU}, Iter: ${
      __ITER + 1
    }, Duration: ${response.timings.duration.toFixed(2)}ms`
  );
  sleep(Math.random() * 2 + 0.5); // 0.5-2.5s random sleep
}

function performHighLatencyRequest() {
  console.log(
    `🐌 Generating high latency request - VU: ${__VU}, Iter: ${__ITER + 1}`
  );

  // Simula una richiesta che potrebbe causare alta latenza
  const response = http.get("http://localhost:8080/api/telemetry/test", {
    timeout: "10s",
    tags: { request_type: "high_latency" },
  });

  highLatencyRequests.add(1);

  const isHighLatency = response.timings.duration > 500;
  if (isHighLatency) {
    tailSamplingTriggers.add(1);
    console.log(
      `🎯 HIGH LATENCY DETECTED: ${response.timings.duration.toFixed(
        2
      )}ms - This should trigger tail sampling!`
    );
  }

  check(response, {
    "high latency request completed": (r) =>
      r.status === 200 || r.status === 404,
    "triggers tail sampling": (r) => r.timings.duration > 500,
  });

  sleep(1);
}

function performErrorRequest() {
  console.log(`❌ Generating error request - VU: ${__VU}, Iter: ${__ITER + 1}`);

  // Richiesta a un endpoint inesistente per generare un errore
  const response = http.get("http://localhost:8080/api/nonexistent/endpoint", {
    timeout: "5s",
    tags: { request_type: "error" },
  });

  errorRequests.add(1);

  const isError = response.status >= 400;
  if (isError) {
    tailSamplingTriggers.add(1);
    console.log(
      `🎯 ERROR DETECTED: Status ${response.status} - This should trigger tail sampling!`
    );
  }

  check(response, {
    "error request generates error": (r) => r.status >= 400,
    "triggers tail sampling for errors": (r) => r.status >= 400,
  });

  sleep(1);
}

export function handleSummary(data) {
  console.log("\n=== RIEPILOGO TEST AVANZATO SAMPLING ===");
  console.log(
    `📊 Richieste normali: ${data.metrics.normal_requests?.values.count || 0}`
  );
  console.log(
    `🐌 Richieste alta latenza: ${
      data.metrics.high_latency_requests?.values.count || 0
    }`
  );
  console.log(
    `❌ Richieste con errore: ${data.metrics.error_requests?.values.count || 0}`
  );
  console.log(
    `🎯 Trigger tail sampling: ${(
      data.metrics.tail_sampling_triggers?.values.rate * 100 || 0
    ).toFixed(2)}%`
  );
  console.log(
    `⏱️  Durata media: ${data.metrics.http_req_duration.values.avg.toFixed(
      2
    )}ms`
  );
  console.log(
    `📈 Durata P95: ${data.metrics.http_req_duration.values["p(95)"].toFixed(
      2
    )}ms`
  );

  console.log("\n=== COSA ASPETTARSI IN JAEGER ===");
  console.log("🔍 URL Jaeger: http://localhost:16686");
  console.log(
    "📋 Head Sampling (1%): Solo ~1% delle richieste normali dovrebbe essere visibile"
  );
  console.log(
    "🎯 Tail Sampling: TUTTE le richieste con errori o latenza >500ms dovrebbero essere visibili"
  );
  console.log(
    "💡 Cerca tracce con tag: request_type=high_latency, request_type=error"
  );

  return {
    "advanced-sampling-test-summary.json": JSON.stringify(data, null, 2),
  };
}
