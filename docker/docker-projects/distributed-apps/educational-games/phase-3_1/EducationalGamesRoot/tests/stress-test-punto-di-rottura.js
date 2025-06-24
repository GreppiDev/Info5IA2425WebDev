import http from "k6/http";
import { check, sleep, group } from "k6";
import { Trend, Counter } from "k6/metrics";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// --- Metriche Personalizzate ---
const loginDuration = new Trend("login_duration");
const getMieClassiDuration = new Trend("get_mie_classi_duration");

// --- Contatori per Errori Specifici ---
const authErrorCounter = new Counter("auth_errors_401");
const loginFailureCounter = new Counter("login_failures");
const logoutFailureCounter = new Counter("logout_failures");
const classiErrorCounter = new Counter("classi_errors");

// --- Funzione per Logging Dettagliato ---
function logError(
  endpoint,
  vu,
  iter,
  status,
  error,
  responseBody = "",
  currentVUs = "unknown"
) {
  const timestamp = new Date().toISOString();
  console.error(
    `[${timestamp}] VU:${vu} ITER:${iter} CurrentVUs:${currentVUs} | ERRORE ${endpoint} | Status:${status} | Error:${error} | Body:${responseBody.substring(
      0,
      200
    )}`
  );
}

function logSuccess(endpoint, vu, iter, duration, currentVUs = "unknown") {
  const timestamp = new Date().toISOString();
  console.log(
    `[${timestamp}] VU:${vu} ITER:${iter} CurrentVUs:${currentVUs} | SUCCESS ${endpoint} | Duration:${duration}ms`
  );
}

// --- Funzione per ottenere VU correnti (approssimazione) ---
function getCurrentVUs() {
  const testStartTime = new Date("2025-06-12T15:42:42+02:00"); // Aggiorna con timestamp reale
  const now = new Date();
  const elapsedMinutes = (now - testStartTime) / (1000 * 60);

  if (elapsedMinutes < 2)
    return Math.floor(elapsedMinutes * 25); // 0-50 in 2min
  else if (elapsedMinutes < 5) return 50; // 50 per 3min
  else if (elapsedMinutes < 7)
    return 50 + Math.floor((elapsedMinutes - 5) * 25); // 50-100 in 2min
  else if (elapsedMinutes < 10) return 100; // 100 per 3min
  else if (elapsedMinutes < 12)
    return 100 + Math.floor((elapsedMinutes - 10) * 50); // 100-200 in 2min
  else if (elapsedMinutes < 15) return 200; // 200 per 3min
  else return Math.max(0, 200 - Math.floor((elapsedMinutes - 15) * 200)); // 200-0 in 1min
}

// --- Opzioni del Test k6 ---
export const options = {
  insecureSkipTLSVerify: true,
  // Scenari di STRESS TEST: l'obiettivo è aumentare il carico fino al punto di rottura.
  // Iniziamo con un carico leggero e aumentiamo costantemente il numero di utenti virtuali.
  stages: [
    { duration: "2m", target: 50 }, // Rampa graduale fino a 50 utenti in 2 minuti
    { duration: "3m", target: 50 }, // Mantiene 50 utenti per 3 minuti per stabilizzare
    { duration: "2m", target: 100 }, // Rampa fino a 100 utenti
    { duration: "3m", target: 100 }, // Mantiene 100 utenti
    { duration: "2m", target: 200 }, // Rampa fino a 200 utenti
    { duration: "3m", target: 200 }, // Mantiene 200 utenti per osservare il comportamento al limite
    { duration: "1m", target: 0 }, // Rampa verso il basso per vedere come il sistema si riprende
  ],
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<800"],
    login_duration: ["p(95)<1000"],
    get_mie_classi_duration: ["p(95)<800"],
    auth_errors_401: ["count<10"], // Massimo 10 errori 401
    login_failures: ["count<5"], // Massimo 5 fallimenti login
    logout_failures: ["count<5"], // Massimo 5 fallimenti logout
    classi_errors: ["count<5"], // Massimo 5 errori classi
  },
};

// --- Funzione Principale del Test ---
export default function () {
  const baseUrl = "https://localhost:8443";
  const uniqueifier = `${__VU}-${__ITER}`;
  const currentVUs = getCurrentVUs();
  const user = {
    email: `testuser_${uniqueifier}@example.com`,
    password: "Password123!",
    nome: "Test",
    cognome: `User_${uniqueifier}`,
    ruolo: "Studente",
  };

  console.log(
    `[START] VU:${__VU} ITER:${__ITER} CurrentVUs:${currentVUs} | Inizio flusso per ${user.email}`
  );

  group("User Flow: Registrazione, Login e Attività", () => {
    let authCookie = null; // Inizializza a null

    // 1. Registrazione Utente
    group("1. Registrazione", () => {
      const registerPayload = JSON.stringify({
        nome: user.nome,
        cognome: user.cognome,
        email: user.email,
        password: user.password,
        ruolo: user.ruolo,
      });
      const registerParams = {
        headers: { "Content-Type": "application/json" },
      };
      const registerRes = http.post(
        `${baseUrl}/api/account/register`,
        registerPayload,
        registerParams
      );
      check(registerRes, {
        "Registrazione ha successo (status 200)": (r) => r.status === 200,
      });

      // Log dettagliato registrazione
      if (registerRes.status === 200) {
        logSuccess(
          "REGISTER",
          __VU,
          __ITER,
          registerRes.timings.duration,
          currentVUs
        );
      } else {
        logError(
          "REGISTER",
          __VU,
          __ITER,
          registerRes.status,
          "Registration failed",
          registerRes.body,
          currentVUs
        );
      }
    });

    sleep(1);

    // 2. Login e Ottenimento Cookie
    group("2. Login", () => {
      const loginPayload = JSON.stringify({
        email: user.email,
        password: user.password,
        rememberMe: false,
      });

      // Impostiamo `redirects: 0` per intercettare manualmente la risposta 302
      const loginParams = {
        headers: { "Content-Type": "application/json" },
        redirects: 0,
      };
      const loginRes = http.post(
        `${baseUrl}/api/account/login`,
        loginPayload,
        loginParams
      );

      loginDuration.add(loginRes.timings.duration); // Verifichiamo che il login restituisca un redirect (302) e che imposti il cookie
      check(loginRes, {
        "Login restituisce un redirect (status 302)": (r) => r.status === 302,
        "Cookie di autenticazione ricevuto nella risposta": (r) =>
          r.cookies[".AspNetCore.Authentication.EducationalGames"] !==
          undefined,
      });

      // Log dettagliato login
      if (loginRes.status === 302) {
        logSuccess(
          "LOGIN",
          __VU,
          __ITER,
          loginRes.timings.duration,
          currentVUs
        );
      } else {
        logError(
          "LOGIN",
          __VU,
          __ITER,
          loginRes.status,
          "Login failed",
          loginRes.body,
          currentVUs
        );
        loginFailureCounter.add(1);
      }

      // Estraiamo il cookie dalla risposta 302
      if (
        loginRes.cookies[".AspNetCore.Authentication.EducationalGames"] &&
        loginRes.cookies[".AspNetCore.Authentication.EducationalGames"].length >
          0
      ) {
        authCookie =
          loginRes.cookies[".AspNetCore.Authentication.EducationalGames"][0];
        console.log(
          `[COOKIE] VU:${__VU} ITER:${__ITER} | Cookie estratto: ${
            authCookie.name
          }=${authCookie.value.substring(0, 20)}...`
        );
      } else {
        logError(
          "LOGIN",
          __VU,
          __ITER,
          loginRes.status,
          "Cookie non trovato",
          "",
          currentVUs
        );
      }
    }); // Se il cookie non è stato ottenuto, interrompi il flusso per questo utente
    if (!authCookie) {
      logError(
        "AUTH",
        __VU,
        __ITER,
        "NO_COOKIE",
        "Cookie di autenticazione non estratto",
        "",
        currentVUs
      );
      loginFailureCounter.add(1);
      return;
    }

    sleep(2); // --- 3. Visualizzazione Classi (come studente autenticato) ---
    group("3. Visualizzazione Classi", () => {
      // Prepariamo gli header con il cookie estratto manualmente
      const authParams = {
        headers: {
          Cookie: `${authCookie.name}=${authCookie.value}`,
        },
      };

      // Ottieni le classi a cui lo studente è iscritto
      const getMieClassiRes = http.get(
        `${baseUrl}/api/iscrizioni/mie`,
        authParams
      );
      getMieClassiDuration.add(getMieClassiRes.timings.duration);
      check(getMieClassiRes, {
        "Ottenute le mie classi (status 200)": (r) => r.status === 200,
        "Risposta contiene array": (r) => {
          try {
            const classi = r.json();
            return Array.isArray(classi);
          } catch (e) {
            console.error(
              `Errore nel parsing del JSON per /api/iscrizioni/mie: ${e}`
            );
            return false;
          }
        },
      });

      // Log dettagliato per endpoint classi
      if (getMieClassiRes.status === 200) {
        logSuccess(
          "CLASSI",
          __VU,
          __ITER,
          getMieClassiRes.timings.duration,
          currentVUs
        );
      } else {
        logError(
          "CLASSI",
          __VU,
          __ITER,
          getMieClassiRes.status,
          "Classi request failed",
          getMieClassiRes.body,
          currentVUs
        );
        classiErrorCounter.add(1);
        if (getMieClassiRes.status === 401) {
          authErrorCounter.add(1);
          console.error(
            `[AUTH_ERROR] VU:${__VU} ITER:${__ITER} | Cookie scaduto o invalido per /api/iscrizioni/mie`
          );
        }
      }

      // Log per debug (opzionale)
      try {
        const classi = getMieClassiRes.json();
        console.log(
          `[CLASSI_DATA] VU:${__VU} ITER:${__ITER} CurrentVUs:${currentVUs} | Studente ${user.email} - Classi trovate: ${classi.length}`
        );
      } catch (e) {
        logError(
          "CLASSI_PARSE",
          __VU,
          __ITER,
          getMieClassiRes.status,
          e.toString(),
          getMieClassiRes.body,
          currentVUs
        );
      }
    });

    sleep(3); // --- 4. Logout ---
    group("4. Logout", () => {
      const logoutParams = {
        headers: { Cookie: `${authCookie.name}=${authCookie.value}` },
      };
      const logoutRes = http.post(
        `${baseUrl}/api/account/logout`,
        null,
        logoutParams
      );

      check(logoutRes, {
        "Logout ha successo (status 204 o 302)": (r) =>
          r.status === 204 || r.status === 302,
      });

      // Log dettagliato logout
      if (logoutRes.status === 204 || logoutRes.status === 302) {
        logSuccess(
          "LOGOUT",
          __VU,
          __ITER,
          logoutRes.timings.duration,
          currentVUs
        );
      } else {
        logError(
          "LOGOUT",
          __VU,
          __ITER,
          logoutRes.status,
          "Logout failed",
          logoutRes.body,
          currentVUs
        );
        logoutFailureCounter.add(1);
        if (logoutRes.status === 401) {
          authErrorCounter.add(1);
          console.error(
            `[AUTH_ERROR] VU:${__VU} ITER:${__ITER} | Cookie scaduto o invalido per logout`
          );
        }
      }
    });

    console.log(
      `[END] VU:${__VU} ITER:${__ITER} CurrentVUs:${currentVUs} | Flusso completato per ${user.email}`
    );
  });
}

// --- Generazione Report HTML ---
export function handleSummary(data) {
  return {
    "summary-stress-test.html": htmlReport(data, {
      title: "Report Performance EducationalGames",
    }),
  };
}
