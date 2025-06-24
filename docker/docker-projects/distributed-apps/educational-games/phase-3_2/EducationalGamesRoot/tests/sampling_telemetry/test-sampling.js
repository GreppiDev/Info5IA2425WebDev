import http from "k6/http";
import { check, sleep } from "k6";
import { Counter, Rate } from "k6/metrics";

// Metriche personalizzate
const samplingTestRequests = new Counter("sampling_test_requests");
const samplingTestErrors = new Rate("sampling_test_errors");

// Configurazione del test
export let options = {
  // Test per verificare il sampling: 20 richieste rapide
  scenarios: {
    sampling_test: {
      executor: "per-vu-iterations",
      vus: 1,
      iterations: 20,
      maxDuration: "30s",
    },
  },
  thresholds: {
    http_req_duration: ["p(95)<1000"], // 95% delle richieste sotto 1 secondo
    sampling_test_errors: ["rate<0.1"], // Meno del 10% di errori
  },
};

export default function () {
  console.log(`Esecuzione richiesta ${__ITER + 1}/20 - VU: ${__VU}`);

  const response = http.get("http://localhost:8080/", {
    timeout: "5s",
    tags: { test_type: "sampling_verification" },
  });

  // Contatori per le metriche
  samplingTestRequests.add(1);

  const isSuccess = check(response, {
    "status is 200": (r) => r.status === 200,
    "response time < 5s": (r) => r.timings.duration < 5000,
  });

  if (!isSuccess) {
    samplingTestErrors.add(1);
    console.error(`Richiesta ${__ITER + 1} fallita: Status ${response.status}`);
  } else {
    console.log(
      `Richiesta ${
        __ITER + 1
      } completata - Durata: ${response.timings.duration.toFixed(2)}ms`
    );
  }

  // Pausa di 100ms tra le richieste
  sleep(0.1);
}

export function handleSummary(data) {
  console.log("\n=== RIEPILOGO TEST SAMPLING ===");
  console.log(
    `Richieste totali: ${data.metrics.sampling_test_requests.values.count}`
  );
  console.log(
    `Errori: ${(data.metrics.sampling_test_errors.values.rate * 100).toFixed(
      2
    )}%`
  );
  console.log(
    `Durata media: ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms`
  );
  console.log(
    `Durata P95: ${data.metrics.http_req_duration.values["p(95)"].toFixed(2)}ms`
  );

  console.log("\n=== ISTRUZIONI VERIFICA ===");
  console.log("🔍 Controlla Jaeger UI per vedere le tracce filtrate:");
  console.log("   URL: http://localhost:16686");
  console.log(
    "📊 Con head sampling al 1%, dovresti vedere circa 0-1 tracce dalle 20 richieste."
  );
  console.log(
    "⚡ Le tracce con errori o latenza > 500ms saranno comunque mantenute dal tail sampling."
  );

  return {
    "sampling-test-summary.json": JSON.stringify(data, null, 2),
  };
}
