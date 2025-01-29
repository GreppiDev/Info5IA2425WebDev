document.addEventListener("DOMContentLoaded", async function () {
    const loadingElement = document.getElementById("loading");
    const errorElement = document.getElementById("error");
    const detailsElement = document.getElementById("movieDetails");
    const saveButton = document.getElementById("saveMovie");

    // Ottieni l'ID del film e la lingua dai parametri URL
    const params = new URLSearchParams(window.location.search);
    const movieId = params.get("id");
    const language = params.get("language") || "it-IT"; // Default a it-IT se non specificato

    if (!movieId) {
        showStatusMessage("ID del film richiesto");
        return;
    }

    try {
        // Verifica se il film esiste già nel database locale
        const existsResponse = await fetch(`/api/films/tmdb/${movieId}`);
        const filmExists = existsResponse.ok;

        // Ottieni i dettagli del film con il parametro della lingua
        const [movieResponse, creditsResponse, videosResponse] = await Promise.all([
            fetch(`/api/tmdb/movie/${movieId}?language=${language}`),
            fetch(`/api/tmdb/movie/${movieId}/credits?language=${language}`),
            fetch(`/api/tmdb/movie/${movieId}/videos?language=${language}`)
        ]);

        if (!movieResponse.ok || !creditsResponse.ok || !videosResponse.ok) {
            throw new Error("Impossibile recuperare i dati del film");
        }

        const [movieData, creditsData, videosData] = await Promise.all([
            movieResponse.json(),
            creditsResponse.json(),
            videosResponse.json()
        ]);

        // Se il film esiste già, disabilita il pulsante di salvataggio
        if (filmExists) {
            saveButton.disabled = true;
            saveButton.textContent = 'Film già salvato';
            saveButton.classList.remove('btn-primary');
            saveButton.classList.add('btn-secondary');
        }

        // Trova il regista
        const director = creditsData.crew.find(person => person.job === "Director");

        // Aggiorna l'interfaccia utente
        document.getElementById("movieTitle").textContent = movieData.title;
        document.getElementById("movieId").textContent = movieData.id;
        document.getElementById("movieDirector").textContent = director ? director.name : "Sconosciuto";
        document.getElementById("movieYear").textContent = new Date(movieData.release_date).getFullYear();
        document.getElementById("movieOverview").textContent = movieData.overview;

        // Imposta il poster del film
        const posterPath = movieData.poster_path;
        if (posterPath) {
            document.getElementById("moviePoster").src = `https://image.tmdb.org/t/p/w500${posterPath}`;
        }

        // Configura il trailer YouTube se disponibile
        const trailer = videosData.results.find(video => 
            video.type === "Trailer" && video.site === "YouTube");
        
        if (trailer) {
            const youtubePlayer = document.getElementById("youtubePlayer");
            youtubePlayer.innerHTML = `
                <iframe width="100%" height="400" 
                    src="https://www.youtube.com/embed/${trailer.key}" 
                    frameborder="0" allowfullscreen>
                </iframe>`;
        } else {
            document.getElementById("trailerContainer").classList.add("d-none");
        }

        // Se il film non esiste nel database locale, configura il salvataggio
        if (!filmExists) {
            // Gestione del pulsante di salvataggio
            saveButton.addEventListener("click", async () => {
                try {
                    saveButton.disabled = true;
                    saveButton.textContent = "Salvataggio...";

                    // Prima, cerca il regista nel database locale
                    if (!director) {
                        throw new Error("Informazioni del regista non trovate");
                    }

                    let localDirector = await findDirectorByTmdbId(director.id);

                    if (!localDirector) {
                        // Ottieni dettagli aggiuntivi del regista da TMDB
                        const directorResponse = await fetch(`/api/tmdb/person/${director.id}?language=${language}`);
                        if (!directorResponse.ok) {
                            throw new Error("Impossibile recuperare i dettagli del regista");
                        }
                        const directorData = await directorResponse.json();

                        // Verifica se abbiamo informazioni complete del regista
                        if (directorData.name && directorData.place_of_birth) {
                            // Prova a dividere il nome in nome e cognome
                            const nameParts = directorData.name.split(" ");
                            const directorInfo = {
                                nome: nameParts[0],
                                cognome: nameParts.slice(1).join(" "),
                                nazionalità: directorData.place_of_birth.split(",").slice(-1)[0].trim(),
                                tmdbId: directorData.id
                            };

                            // Salva il regista nel database
                            localDirector = await saveDirector(directorInfo);
                        } else {
                            // Mostra il modale per l'input manuale
                            currentDirectorData = {
                                tmdbId: director.id,
                                name: director.name
                            };
                            
                            // Pre-compila il nome se disponibile
                            if (director.name) {
                                const nameParts = director.name.split(" ");
                                document.getElementById("directorName").value = nameParts[0];
                                document.getElementById("directorSurname").value = nameParts.slice(1).join(" ");
                            }
                            document.getElementById("directorTmdbId").value = director.id;
                            
                            directorModal.show();
                            return;
                        }
                    }

                    // Salva il film con l'ID del regista
                    await saveMovie(movieData, localDirector.id);

                    // Mostra il messaggio di successo
                    saveButton.textContent = "Salvato!";
                    saveButton.classList.remove("btn-primary");
                    saveButton.classList.add("btn-success");
                    saveButton.disabled = true;
                } catch (error) {
                    console.error("Errore di salvataggio:", error);
                    saveButton.textContent = "Salvataggio fallito";
                    saveButton.classList.remove("btn-primary");
                    saveButton.classList.add("btn-danger");
                    saveButton.disabled = false;
                }
            });
        }

        // Mostra i dettagli
        detailsElement.classList.remove("d-none");
    } catch (error) {
        console.error("Errore:", error);
        showStatusMessage("Impossibile caricare i dettagli del film");
    } finally {
        loadingElement.classList.add("d-none");
    }

    // Funzioni di utilità
    function showStatusMessage(message, type = "danger") {
        loadingElement.classList.add("d-none");
        errorElement.textContent = message;
        errorElement.classList.remove("d-none");
    }

    async function findDirectorByTmdbId(tmdbId) {
        try {
            const response = await fetch(`/api/registi/tmdb/${tmdbId}`);
            if (response.ok) {
                return await response.json();
            }
            return null;
        } catch (error) {
            console.error("Errore nella ricerca del regista:", error);
            return null;
        }
    }

    async function saveDirector(directorData) {
        const response = await fetch("/api/registi", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(directorData)
        });

        if (!response.ok) {
            throw new Error("Impossibile salvare il regista");
        }

        return await response.json();
    }

    async function saveMovie(movieData, registaId) {
        const filmData = {
            titolo: movieData.title,
            dataProduzione: movieData.release_date,
            registaId: registaId,
            durata: movieData.runtime || 0,
            tmdbId: movieData.id
        };

        const response = await fetch("/api/films", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(filmData)
        });

        if (!response.ok) {
            throw new Error("Impossibile salvare il film");
        }

        return await response.json();
    }
});
