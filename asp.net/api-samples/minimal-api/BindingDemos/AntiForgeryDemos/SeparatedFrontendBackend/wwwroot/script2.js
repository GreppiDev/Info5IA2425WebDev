document.addEventListener("DOMContentLoaded", function () {
  let antiforgeryToken = null; // Variabile per memorizzare l'Anti-Forgery Token

  // ottiene l'Anti-Forgery Token dal backend al caricamento della pagina o del form
  //accede all'endpoint stateless (non cambia nulla per il frontend)
  fetch("/antiforgery/token2")
	.then((response) => response.text())
	.then((token) => {
	  antiforgeryToken = token; // Memorizza il token
	  console.log("Anti-Forgery Token ottenuto:", antiforgeryToken);
	})
	.catch((error) => {
	  console.error("Errore nel recupero del token Anti-Forgery:", error);
	  alert(
		"Errore nel recupero del token Anti-Forgery. L'applicazione potrebbe non funzionare correttamente."
	  );
	});

  const dataForm = document.getElementById("dataForm");
  dataForm.addEventListener("submit", async function (event) {
	event.preventDefault();

	const messaggioInput = document.getElementById("messaggio");
	const messaggio = messaggioInput.value;

	const formData = new FormData();
	formData.append("messaggio", messaggio); // Prepara i dati del form

	//Si potrebbe richiedere il token anche prima di sottomettere il form

	//Decidere quale opzione applicare (se richiederlo al caricamento del form oppure prima
	//della sottomissione del form) dipende da come si gestiscono i token e le pagine del frontend

	//Il codice commentato sotto richiede il token prima di sottomettere il form
	//va usato in alternativa alla richiesta del token al caricamento della pagina/form

	// try {
	// 	// Prima richiedi il token usando await
	// 	const tokenResponse = await fetch("/antiforgery/token2");
	// 	antiforgeryToken = await tokenResponse.text();
	// } catch (error) {
	// 	console.error("Errore nel recupero del token:", error);
	// 	alert("Errore nel recupero del token. Riprova più tardi.");
	// 	return;
	// }

	// Sottomissione del form con il pattern .then() originale
	fetch("/api/submitData", {
	  method: "POST",
	  headers: {
		"Content-Type": "application/x-www-form-urlencoded",
		RequestVerificationToken: antiforgeryToken,
	  },
	  body: new URLSearchParams(formData).toString(),
	})
	  .then((response) => response.json())
	  .then((data) => {
		alert(data.message);
	  })
	  .catch((error) => {
		console.error("Errore nell'invio dei dati:", error);
		alert("Errore nell'invio dei dati. Riprova più tardi.");
	  });
  });
});
