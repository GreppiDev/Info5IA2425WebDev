import http from "k6/http";
import { check, sleep, group } from "k6";

export const options = {
  insecureSkipTLSVerify: true,
  stages: [
    { duration: "10s", target: 1 }, // 1 solo utente virtuale
  ],
};

export default function () {
  const baseUrl = "https://localhost:8443";
  // Genera un utente univoco per ogni esecuzione del test
  const uniqueifier = new Date().getTime();
  const teacherEmail = `teacher_${uniqueifier}@test.com`;
  const password = "Password123!";
  let authCookie = null;
  let classeId = null;

  console.log(`Inizio flusso di verifica telemetria per ${teacherEmail}`);

  group("1. Registrazione Docente", () => {
    const registerPayload = JSON.stringify({
      nome: "Telemetria",
      cognome: `Docente_${uniqueifier}`,
      email: teacherEmail,
      password: password,
      ruolo: "Docente",
    });
    const registerRes = http.post(
      `${baseUrl}/api/account/register`,
      registerPayload,
      {
        headers: { "Content-Type": "application/json" },
      }
    );
    check(registerRes, {
      "Registrazione Docente OK (200)": (r) => r.status === 200,
    });
  });

  sleep(1);

  group("2. Login Docente", () => {
    const loginPayload = JSON.stringify({
      email: teacherEmail,
      password: password,
    });
    const loginRes = http.post(`${baseUrl}/api/account/login`, loginPayload, {
      headers: { "Content-Type": "application/json" },
      redirects: 0, // Intercetta il redirect per prendere il cookie
    });
    check(loginRes, { "Login Docente OK (302)": (r) => r.status === 302 });
    if (
      loginRes.cookies[".AspNetCore.Authentication.EducationalGames"]?.length >
      0
    ) {
      authCookie =
        loginRes.cookies[".AspNetCore.Authentication.EducationalGames"][0];
    }
  });

  if (!authCookie) {
    console.error(
      "Login fallito, impossibile continuare il test di telemetria."
    );
    return;
  }

  sleep(1);

  group("3. Creazione Classe", () => {
    const creaClassePayload = JSON.stringify({
      nomeClasse: `Classe di Telemetria ${uniqueifier}`,
      materiaId: 1,
    });
    const res = http.post(`${baseUrl}/api/classi`, creaClassePayload, {
      headers: {
        "Content-Type": "application/json",
        Cookie: `${authCookie.name}=${authCookie.value}`,
      },
    });
    check(res, { "Creazione Classe OK (201)": (r) => r.status === 201 });
    if (res.status === 201) {
      classeId = res.json("id");
      console.log(`Classe creata con ID: ${classeId}`);
    }
  });

  if (!classeId) {
    console.error("Creazione classe fallita, impossibile continuare.");
    return;
  }

  sleep(1);

  group("4. Associazione Gioco", () => {
    const associaGiocoPayload = JSON.stringify({ giocoId: 1 }); // Associa gioco con ID 1
    const res = http.post(
      `${baseUrl}/api/classi/${classeId}/giochi`,
      associaGiocoPayload,
      {
        headers: {
          "Content-Type": "application/json",
          Cookie: `${authCookie.name}=${authCookie.value}`,
        },
      }
    );
    check(res, { "Associazione Gioco OK (204)": (r) => r.status === 204 });
  });

  sleep(1);

  group("5. Verifica Classe", () => {
    const res = http.get(`${baseUrl}/api/classi/${classeId}`, {
      headers: { Cookie: `${authCookie.name}=${authCookie.value}` },
    });
    check(res, {
      "Dettaglio Classe OK (200)": (r) => r.status === 200,
      "Gioco risulta associato": (r) => r.json("giochiAssociati.0.id") === 1,
    });
  });

  console.log("Flusso di verifica telemetria completato.");
}
