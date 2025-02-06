/**
 * Funzioni di geocoding per la conversione di indirizzi e coordinate
 * Utilizza le API di OpenStreetMap Nominatim
 * Documentazione: https://nominatim.org/release-docs/develop/api/Overview/
 *
 * Note sull'implementazione:
 * - Nominatim richiede un User-Agent header valido
 * - Le coordinate sono restituite in formato [latitudine, longitudine]
 * - Rispetta i limiti di utilizzo di Nominatim (max 1 richiesta al secondo)
 * - Gestione degli errori integrata per richieste fallite o dati mancanti
 */

// Global variables
let map;
let currentMarker = null;
const loadingMessage = document.getElementById("loadingMessage");
const errorMessage = document.getElementById("errorMessage");

// Rate limiter for Nominatim requests (1 request per second)
let lastRequestTime = 0;
async function rateLimitRequest() {
    const now = Date.now();
    const timeSinceLastRequest = now - lastRequestTime;
    if (timeSinceLastRequest < 1000) {
        await new Promise(resolve => setTimeout(resolve, 1000 - timeSinceLastRequest));
    }
    lastRequestTime = Date.now();
}

// Show/hide loading message
function toggleLoading(show) {
    loadingMessage.style.display = show ? "block" : "none";
}

// Show error message
function showError(message) {
    console.error(message);
    errorMessage.textContent = message;
    errorMessage.style.display = "block";
    setTimeout(() => {
        errorMessage.style.opacity = '0';
        setTimeout(() => {
            errorMessage.style.display = "none";
            errorMessage.style.opacity = '1';
        }, 300);
    }, 5000);
}

// Get MapTiler API key from backend (still needed for map tiles)
async function getMapTilerKey() {
    try {
        const response = await fetch('/api/maps/map-tiler-key');
        if (!response.ok) throw new Error('Failed to fetch API key');
        const data = await response.json();
        return data.key;
    } catch (error) {
        console.error('Error fetching MapTiler key:', error);
        showError('Error initializing terrain layer');
        return null;
    }
}

// Initialize map
async function initializeMap() {
    toggleLoading(true);

    try {
        // Get MapTiler key before initializing the map
        const mapTilerKey = await getMapTilerKey();
        if (!mapTilerKey) {
            throw new Error('Failed to initialize terrain layer: Missing API key');
        }

        // Create map with Greppi Institute coordinates [lat, lng]
        map = L.map('myMap', {
            center: [45.704011, 9.304927],
            zoom: 16,
            minZoom: 3,
            maxZoom: 19
        });

        // Define base layers
        const baseMapLayers = {
            "Streets": L.tileLayer(`https://api.maptiler.com/maps/streets-v2/{z}/{x}/{y}.png?key=${mapTilerKey}`, {
                maxZoom: 19,
                attribution: '© MapTiler © OpenStreetMap contributors'
            }),
            "Terrain": L.tileLayer(`https://api.maptiler.com/maps/topo-v2/256/{z}/{x}/{y}.png?key=${mapTilerKey}`, {
                maxZoom: 19,
                attribution: '© MapTiler © OpenStreetMap contributors'
            }),
            "Satellite": L.tileLayer(`https://api.maptiler.com/tiles/satellite-v2/{z}/{x}/{y}.jpg?key=${mapTilerKey}`, {
                maxZoom: 19,
                attribution: '© MapTiler © OpenStreetMap contributors'
            })
        };

        // Add default layer
        baseMapLayers["Streets"].addTo(map);

        // Add layer control
        L.control.layers(baseMapLayers, {}, {
            position: 'topright'
        }).addTo(map);

        // Add zoom control to top-right
        map.zoomControl.remove();
        L.control.zoom({ position: 'topright' }).addTo(map);

        // Add scale control
        L.control.scale({ position: 'bottomright' }).addTo(map);

        // Handle map ready
        map.whenReady(() => {
            toggleLoading(false);
            errorMessage.style.display = "none";
        });

    } catch (error) {
        console.error("Error initializing map:", error);
        showError("Failed to initialize map: " + error.message);
        toggleLoading(false);
    }
}

// Geocoding with Nominatim (Address search)
async function searchAddress() {
    const searchInput = document.getElementById("searchAddress").value;
    if (!searchInput) {
        showError("Inserire un indirizzo da cercare");
        return;
    }

    try {
        toggleLoading(true);
        await rateLimitRequest();

        const response = await fetch(
            `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(searchInput)}&countrycodes=it&limit=1`,
            {
                headers: {
                    'User-Agent': 'MapTilerToOSM_SchoolProject/1.0'
                }
            }
        );

        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const data = await response.json();
        if (data && data.length > 0) {
            const result = data[0];
            const lat = parseFloat(result.lat);
            const lon = parseFloat(result.lon);
            const address = result.display_name;

            addMarker([lat, lon], address);
            map.setView([lat, lon], 15);

            document.getElementById("searchResults").innerHTML = `Trovato: ${address}`;
        } else {
            document.getElementById("searchResults").innerHTML = "Nessun risultato";
            showError("Indirizzo non trovato");
        }
    } catch (error) {
        showError("Errore ricerca: " + error.message);
    } finally {
        toggleLoading(false);
    }
}

// Address to Coordinates conversion
async function convertAddressToCoords() {
    const addressInput = document.getElementById("addressInput").value;
    if (!addressInput) {
        showError("Inserire un indirizzo da convertire");
        return;
    }

    try {
        toggleLoading(true);
        await rateLimitRequest();

        const response = await fetch(
            `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(addressInput)}&countrycodes=it&limit=1`,
            {
                headers: {
                    'User-Agent': 'MapTilerToOSM_SchoolProject/1.0'
                }
            }
        );

        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const data = await response.json();
        if (data && data.length > 0) {
            const result = data[0];
            const lat = parseFloat(result.lat);
            const lon = parseFloat(result.lon);
            const address = result.display_name;

            document.getElementById("latitude").textContent = lat.toFixed(6);
            document.getElementById("longitude").textContent = lon.toFixed(6);

            addMarker([lat, lon], address);
            map.setView([lat, lon], 15);
        } else {
            document.getElementById("latitude").textContent = "-";
            document.getElementById("longitude").textContent = "-";
            showError("Coordinate non trovate");
        }
    } catch (error) {
        showError("Errore conversione: " + error.message);
    } finally {
        toggleLoading(false);
    }
}

// Coordinates to Address conversion
async function convertCoordsToAddress() {
    const lat = parseFloat(document.getElementById("latInput").value);
    const lng = parseFloat(document.getElementById("lngInput").value);

    if (isNaN(lat) || isNaN(lng)) {
        showError("Coordinate non valide");
        return;
    }

    try {
        toggleLoading(true);
        await rateLimitRequest();

        const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`,
            {
                headers: {
                    'User-Agent': 'MapTilerToOSM_SchoolProject/1.0'
                }
            }
        );

        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const data = await response.json();
        if (data && data.display_name) {
            const address = data.display_name;
            document.getElementById("addressResult").innerHTML = address;

            addMarker([lat, lng], address);
            map.setView([lat, lng], 15);
        } else {
            document.getElementById("addressResult").innerHTML = "Nessun indirizzo trovato";
            showError("Indirizzo non trovato");
        }
    } catch (error) {
        showError("Errore conversione: " + error.message);
        document.getElementById("addressResult").innerHTML = "Errore durante la conversione";
    } finally {
        toggleLoading(false);
    }
}

// Add marker to map
function addMarker(coords, title) {
    if (currentMarker) {
        map.removeLayer(currentMarker);
    }

    currentMarker = L.marker(coords, {
        title: title,
        autoPan: true
    }).addTo(map)
        .bindPopup(title);

    // Ensure coordinates are numbers
    const lat = parseFloat(coords[0]);
    const lng = parseFloat(coords[1]);
    updateCurrentMarker([lat, lng], title);
}

// Update current marker display
function updateCurrentMarker(coordinates, address) {
    const lat = parseFloat(coordinates[0]);
    const lng = parseFloat(coordinates[1]);

    document.getElementById("currentMarker").innerHTML = `
        <div>Località: ${address}</div>
        <div>Coordinate: ${lat.toFixed(6)}, ${lng.toFixed(6)}</div>
    `;
}

// Clear markers
function clearMarkers() {
    if (currentMarker) {
        map.removeLayer(currentMarker);
        currentMarker = null;
    }
    document.getElementById("currentMarker").innerHTML = "Nessun marker inserito";
    document.getElementById("searchResults").innerHTML = "";
    document.getElementById("addressResult").innerHTML = "";
    document.getElementById("latitude").textContent = "-";
    document.getElementById("longitude").textContent = "-";
}

// Initialize map on load
document.addEventListener("DOMContentLoaded", initializeMap);

// Handle window resize
window.addEventListener("resize", () => {
    if (map) {
        map.invalidateSize();
    }
});
