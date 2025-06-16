import http from "k6/http";
import { check, sleep, group } from "k6";
import { Trend } from "k6/metrics";
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// --- Metriche Personalizzate ---
const loginDuration = new Trend("login_duration");
const getMieClassiDuration = new Trend("get_mie_classi_duration");

// --- Opzioni del Test k6 ---
export const options = {
  insecureSkipTLSVerify: true,
  stages: [
    { duration: "30s", target: 10 },
    { duration: "1m", target: 10 },
    { duration: "30s", target: 20 },
    { duration: "2m", target: 20 },
    { duration: "30s", target: 0 },
  ],
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<800"],
    login_duration: ["p(95)<1000"],
    get_mie_classi_duration: ["p(95)<800"],
  },
};

// --- Funzione Principale del Test ---
export default function () {
  const baseUrl = "https://localhost:8443";
  const uniqueifier = `${__VU}-${__ITER}`;
  const user = {
    email: `testuser_${uniqueifier}@example.com`,
    password: "Password123!",
    nome: "Test",
    cognome: `User_${uniqueifier}`,
    ruolo: "Studente",
  };

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

      loginDuration.add(loginRes.timings.duration);

      // Verifichiamo che il login restituisca un redirect (302) e che imposti il cookie
      check(loginRes, {
        "Login restituisce un redirect (status 302)": (r) => r.status === 302,
        "Cookie di autenticazione ricevuto nella risposta": (r) =>
          r.cookies[".AspNetCore.Authentication.EducationalGames"] !==
          undefined,
      });

      // Estraiamo il cookie dalla risposta 302
      if (
        loginRes.cookies[".AspNetCore.Authentication.EducationalGames"] &&
        loginRes.cookies[".AspNetCore.Authentication.EducationalGames"].length >
          0
      ) {
        authCookie =
          loginRes.cookies[".AspNetCore.Authentication.EducationalGames"][0];
      }
    });

    // Se il cookie non è stato ottenuto, interrompi il flusso per questo utente
    if (!authCookie) {
      console.error(
        `Login fallito: il cookie di autenticazione non è stato estratto per l'utente ${user.email}. Interruzione del flusso.`
      );
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

      // Log per debug (opzionale)
      try {
        const classi = getMieClassiRes.json();
        console.log(
          `Studente ${user.email} - Classi trovate: ${classi.length}`
        );
      } catch (e) {
        console.error(
          `Errore nel parsing del JSON per /api/iscrizioni/mie: ${e}. Corpo della risposta: ${getMieClassiRes.body}`
        );
      }
    });

    sleep(3);

    // --- 4. Logout ---
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
    });
  });
}

// --- Generazione Report HTML ---
export function handleSummary(data) {
  return {
    "summary.html": htmlReport(data, {
      title: "Report Performance EducationalGames",
    }),
  };
}
