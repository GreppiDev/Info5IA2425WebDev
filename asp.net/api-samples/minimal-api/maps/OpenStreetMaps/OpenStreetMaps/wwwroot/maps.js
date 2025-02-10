// Global variables
let map;
let currentMarker = null;
const loadingMessage = document.getElementById("loadingMessage");
const errorMessage = document.getElementById("errorMessage");

// Function to fetch the ArcGIS Location Platform token from the server
async function getArcGISLocationPlatformToken() {
  try {
    const response = await fetch("/api/maps/arcgis-token");
    if (!response.ok) {
      throw new Error("Failed to fetch ArcGIS token");
    }
    const token = await response.json();
    return token.token;
  } catch (error) {
    console.error("Error fetching ArcGIS Location Platform token:", error);
    throw error;
  }
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

// Initialize map
async function initializeMap() {
  toggleLoading(true);

  try {
    // Create map with Greppi Institute coordinates [lat, lng]
    map = L.map('myMap', {
      center: [45.704011, 9.304927],
      zoom: 13,
      minZoom: 3,
      maxZoom: 19
    });

    // Get ESRI token
    const accessToken = await getArcGISLocationPlatformToken();

    // Define base layers
    const baseMapLayers = {
      OpenStreetMap: L.tileLayer(
        "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        {
          maxZoom: 18,
          attribution: "© OpenStreetMap contributors",
        }
      ),
      Satellite: L.esri.Vector.vectorBasemapLayer("arcgis/imagery/standard", {
        token: accessToken,
        maxZoom: 19,
      }),
    };

    // Add default layer
    baseMapLayers["OpenStreetMap"].addTo(map);

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
    if (error.message.includes("ArcGIS token")) {
      showError("Failed to initialize satellite layer: " + error.message);
    } else {
      showError("Failed to initialize map: " + error.message);
    }
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
    const response = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(searchInput)}&countrycodes=it&limit=1`,
      {
        headers: {
          'User-Agent': 'OpenStreetMapOSM/1.0'
        }
      }
    );

    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    
    const data = await response.json();
    if (data.length > 0) {
      const result = data[0];
      const lat = parseFloat(result.lat);
      const lng = parseFloat(result.lon);
      
      addMarker([lat, lng], result.display_name);
      map.setView([lat, lng], 15);
      
      document.getElementById("searchResults").innerHTML = `Trovato: ${result.display_name}`;
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
    const response = await fetch(
      `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(addressInput)}&countrycodes=it&limit=1`,
      {
        headers: {
          'User-Agent': 'OpenStreetMapOSM/1.0'
        }
      }
    );

    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    
    const data = await response.json();
    if (data.length > 0) {
      const result = data[0];
      const lat = parseFloat(result.lat);
      const lng = parseFloat(result.lon);
      
      document.getElementById("latitude").textContent = lat.toFixed(6);
      document.getElementById("longitude").textContent = lng.toFixed(6);
      
      addMarker([lat, lng], result.display_name);
      map.setView([lat, lng], 15);
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
    const response = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`,
      {
        headers: {
          'User-Agent': 'OpenStreetMapOSM/1.0'
        }
      }
    );

    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    
    const data = await response.json();
    if (data.display_name) {
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
