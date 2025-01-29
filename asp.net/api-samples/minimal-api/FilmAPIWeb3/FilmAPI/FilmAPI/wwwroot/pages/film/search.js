document.addEventListener('DOMContentLoaded', function() {
    const searchForm = document.getElementById('searchForm');
    const loadingElement = document.getElementById('loading');
    const errorElement = document.getElementById('error');
    const resultsElement = document.getElementById('results');
    const resultsBody = document.getElementById('resultsBody');
    const localFilmsBody = document.getElementById('localFilmsBody');
    const localFilmsSection = document.getElementById('localFilmsSection');
    const viewToggle = document.getElementById('viewToggle');
    const toggleLabel = document.getElementById('toggleLabel');
    const searchSection = document.getElementById('searchSection');

    // Handle toggle switch change
    viewToggle.addEventListener('change', async function() {
        if (this.checked) {
            // Switch to search view
            localFilmsSection.classList.add('d-none');
            searchSection.classList.remove('d-none');
            toggleLabel.textContent = 'Show Local Films';
            // Clear search results when switching views
            resultsElement.classList.add('d-none');
        } else {
            // Switch to local films view
            searchSection.classList.add('d-none');
            localFilmsSection.classList.remove('d-none');
            toggleLabel.textContent = 'Show TMDB Search';
            await loadLocalFilms();
            
            // Clear any search results
            resultsElement.classList.add('d-none');
        }
    });

    // Load local films from database
    async function loadLocalFilms() {
        try {
            const response = await fetch('/api/films');
            if (!response.ok) throw new Error('Failed to fetch local films');
            const films = await response.json();

            // Load all directors first
            const directorsResponse = await fetch('/api/registi');
            if (!directorsResponse.ok) throw new Error('Impossibile recuperare i registi');
            const directors = await directorsResponse.json();
            
            // Create a map of director IDs to names
            const directorsMap = new Map(
                directors.map(d => [d.id, `${d.nome} ${d.cognome}`])
            );

            // Generate table with director names already included
            localFilmsBody.innerHTML = films.map(film => `
                <tr>
                    <td>${film.titolo}</td>
                    <td>${directorsMap.get(film.registaId) || 'Regista non trovato'}</td>
                    <td>${new Date(film.dataProduzione).toLocaleDateString()}</td>
                    <td>${film.durata} min</td>
                    <td>
                        ${film.tmdbId ?
                            `<a href="movie-details.html?id=${film.tmdbId}" class="btn btn-info btn-sm me-1">
                                <i class="bi bi-eye"></i> View
                            </a>` : ''}
                        <button class="btn btn-warning btn-sm me-1" onclick="editFilm(${film.id})">
                            <i class="bi bi-pencil"></i> Edit
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="deleteFilm(${film.id})">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </td>
                </tr>
            `).join('');
        } catch (error) {
            console.error('Error loading local films:', error);
            errorElement.textContent = 'Failed to load local films';
            errorElement.classList.remove('d-none');
        }
    }

    // Handle search form submit
    searchForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const query = document.getElementById('searchQuery').value;
        const year = document.getElementById('releaseYear').value;
        const language = document.getElementById('language').value;

        // Show loading state
        loadingElement.classList.remove('d-none');
        errorElement.classList.add('d-none');
        resultsElement.classList.add('d-none');

        try {
            // Build search query parameters
            let searchParams = new URLSearchParams();
            searchParams.append('query', query);
            if (language) {
                searchParams.append('language', language);
                // Also set the region to match the language
                searchParams.append('region', language.split('-')[1]);
            }
            
            // Add year filter if provided
            if (year) searchParams.append('primary_release_year', year);

            // Search movies
            const searchResponse = await fetch(`/api/tmdb/search/movie?${searchParams.toString()}`);
            if (!searchResponse.ok) throw new Error('Failed to search movies');
            const searchData = await searchResponse.json();

            // Process results and get director info
            const movies = await Promise.all(searchData.results.map(async movie => {
                try {
                    // Get movie credits to find director
                    const creditsResponse = await fetch(`/api/tmdb/movie/${movie.id}/credits`);
                    if (!creditsResponse.ok) return null;
                    const creditsData = await creditsResponse.json();
                    
                    const director = creditsData.crew.find(person => person.job === 'Director');
                    
                    return {
                        id: movie.id,
                        title: movie.title,
                        director: director ? director.name : 'Unknown',
                        releaseYear: movie.release_date ? new Date(movie.release_date).getFullYear() : 'Unknown',
                        language: movie.original_language.toUpperCase()
                    };
                } catch (error) {
                    console.error('Error fetching movie credits:', error);
                    return null;
                }
            }));

            // Filter out failed requests
            const validMovies = movies.filter(movie => movie !== null);

            // Display results
            resultsBody.innerHTML = validMovies.map(movie => `
                <tr>
                    <td>${movie.title}</td>
                    <td>${movie.director}</td>
                    <td>${movie.releaseYear}</td>
                    <td>${movie.language}</td>
                    <td>
                        <a href="movie-details.html?id=${movie.id}&language=${language}" class="btn btn-primary btn-sm">View Details</a>
                    </td>
                </tr>
            `).join('');

            resultsElement.classList.remove('d-none');
        } catch (error) {
            console.error('Search error:', error);
            errorElement.textContent = 'An error occurred while searching movies. Please try again.';
            errorElement.classList.remove('d-none');
        } finally {
            loadingElement.classList.add('d-none');
        }
    });

    // Delete film function
    window.deleteFilm = async function(id) {
        if (!confirm('Are you sure you want to delete this film?')) return;

        try {
            const response = await fetch(`/api/films/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Failed to delete film');
            await loadLocalFilms();
        } catch (error) {
            console.error('Error deleting film:', error);
            alert('Failed to delete film');
        }
    };

    const editFilmModal = new bootstrap.Modal(document.getElementById('editFilmModal'));
    const editFilmForm = document.getElementById('editFilmForm');
    const saveEditFilmBtn = document.getElementById('saveEditFilmBtn');
    const editRegistaIdSelect = document.getElementById('editRegistaId');

    // Edit film function
    window.editFilm = async function(id) {
        try {
            // Load directors for dropdown
            const registiResponse = await fetch('/api/registi');
            if (!registiResponse.ok) throw new Error('Impossibile recuperare i registi');
            const registi = await registiResponse.json();

            editRegistaIdSelect.innerHTML = registi.map(regista =>
                `<option value="${regista.id}">${regista.nome} ${regista.cognome}</option>`
            ).join('');

            // Load film data
            const filmResponse = await fetch(`/api/films/${id}`);
            if (!filmResponse.ok) throw new Error('Impossibile recuperare i dati del film');
            const film = await filmResponse.json();

            // Populate form
            document.getElementById('editFilmId').value = film.id;
            document.getElementById('editTitolo').value = film.titolo;
            document.getElementById('editDataProduzione').value = film.dataProduzione.split('T')[0];
            document.getElementById('editDurata').value = film.durata;
            document.getElementById('editRegistaId').value = film.registaId;

            // Show modal
            editFilmModal.show();
        } catch (error) {
            console.error('Errore nel caricamento del film:', error);
            alert('Impossibile caricare i dati del film');
        }
    };

    // Handle save edit button click
    saveEditFilmBtn.addEventListener('click', async function() {
        if (!editFilmForm.checkValidity()) {
            editFilmForm.reportValidity();
            return;
        }

        try {
            const filmId = document.getElementById('editFilmId').value;
            const filmData = {
                id: parseInt(filmId),
                titolo: document.getElementById('editTitolo').value,
                dataProduzione: document.getElementById('editDataProduzione').value,
                durata: parseInt(document.getElementById('editDurata').value),
                registaId: parseInt(document.getElementById('editRegistaId').value)
            };

            const response = await fetch(`/api/films/${filmId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(filmData)
            });

            if (!response.ok) throw new Error('Impossibile aggiornare il film');

            // Close modal and reload films
            editFilmModal.hide();
            await loadLocalFilms();
        } catch (error) {
            console.error('Errore durante il salvataggio:', error);
            alert('Impossibile salvare le modifiche');
        }
    });

    // Initial state setup - start with local films view
    searchSection.classList.add('d-none');
    localFilmsSection.classList.remove('d-none');
    viewToggle.checked = false;
    toggleLabel.textContent = 'Show TMDB Search';
    loadLocalFilms(); // Load local films immediately
});