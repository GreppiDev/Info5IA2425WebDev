// Global variables
let map;
let datasource;
let subscriptionKey;
const loadingMessage = document.getElementById("loadingMessage");
const errorMessage = document.getElementById("errorMessage");

// Show/hide loading message
function toggleLoading(show) {
  loadingMessage.style.display = show ? "block" : "none";
}

// Show error message
function showError(message) {
  console.error(message);
  errorMessage.textContent = message;
  errorMessage.style.display = "block";
  // Bootstrap 5 auto-hide with fade effect
  setTimeout(() => {
    errorMessage.style.opacity = '0';
    setTimeout(() => {
      errorMessage.style.display = "none";
      errorMessage.style.opacity = '1';
    }, 300);
  }, 5000);
}

// Initialize map
function initializeMap() {
  const loadingTimeout = setTimeout(() => {
    toggleLoading(false);
  }, 10000);

  toggleLoading(true);

  if (typeof atlas === "undefined") {
    clearTimeout(loadingTimeout);
    showError("Azure Maps SDK not loaded");
    toggleLoading(false);
    return;
  }

  fetch("/api/maps/azure-maps-key")
    .then((response) => {
      if (!response.ok) {
        throw new Error("Failed to fetch authentication token");
      }
      return response.json();
    })
    .then((data) => {
      subscriptionKey = data.key;
      
      map = new atlas.Map("myMap", {
        center: [9.304927, 45.704011], // Coordinate dell'Istituto Greppi
        zoom: 13,
        language: "it-IT",
        view: "Auto",
        authOptions: {
          authType: "subscriptionKey",
          subscriptionKey: data.key,
        },
        style: "road",
      });

      map.events.addOnce("ready", () => {
        clearTimeout(loadingTimeout);

        try {
          datasource = new atlas.source.DataSource();
          map.sources.add(datasource);

          map.layers.add(
            new atlas.layer.SymbolLayer(datasource, null, {
              iconOptions: {
                image: "pin-round-red",
                anchor: "center",
                allowOverlap: true,
              },
            })
          );

          map.controls.add(
            [
              new atlas.control.ZoomControl(),
              new atlas.control.CompassControl(),
              new atlas.control.StyleControl(),
              new atlas.control.PitchControl()
            ],
            {
              position: "top-right"
            }
          );

          toggleLoading(false);
        } catch (error) {
          console.error("Error in map initialization:", error);
          toggleLoading(false);
        }
      });

      map.events.add("error", (e) => {
        console.error("Map error:", e);
        toggleLoading(false);
      });
    })
    .catch((error) => {
      clearTimeout(loadingTimeout);
      console.error("Initialization error:", error);
      showError(error.message);
      toggleLoading(false);
    });
}

// Search address and add marker
async function searchAddress() {
  const searchInput = document.getElementById("searchAddress").value;
  if (!searchInput) {
    showError("Please enter an address to search");
    return;
  }

  if (!subscriptionKey) {
    showError("Map not initialized properly. Please refresh the page.");
    return;
  }

  try {
    toggleLoading(true);
    console.log("Searching for address:", searchInput);

    const searchParams = new URLSearchParams({
      'subscription-key': subscriptionKey,
      'api-version': '1.0',
      'language': 'it-IT',
      'countrySet': 'IT',
      'query': searchInput,
      'limit': '1'
    });

    const response = await fetch(
      `https://atlas.microsoft.com/search/address/json?${searchParams.toString()}`,
      {
        headers: {
          'Accept': 'application/json'
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Search failed: ${response.statusText}`);
    }

    const data = await response.json();
    console.log("Search response:", data);

    if (data.results && data.results.length > 0) {
      const result = data.results[0];
      const coordinates = [result.position.lon, result.position.lat];
      const address = result.address.freeformAddress;

      console.log("Found location:", address, coordinates);

      addMarker(coordinates, address);
      map.setCamera({
        center: coordinates,
        zoom: 15,
        type: "fly",
      });

      document.getElementById("searchResults").innerHTML = `Found: ${address}`;
    } else {
      console.log("No results found");
      document.getElementById("searchResults").innerHTML = "No results found";
      showError("No matching address found");
    }
  } catch (error) {
    console.error("Search error:", error);
    showError("Failed to search address: " + error.message);
  } finally {
    toggleLoading(false);
  }
}

// Convert address to coordinates
async function convertAddressToCoords() {
  const addressInput = document.getElementById("addressInput").value;
  if (!addressInput) {
    showError("Please enter an address to convert");
    return;
  }

  if (!subscriptionKey) {
    showError("Map not initialized properly. Please refresh the page.");
    return;
  }

  try {
    toggleLoading(true);
    console.log("Converting address to coordinates:", addressInput);

    const searchParams = new URLSearchParams({
      'subscription-key': subscriptionKey,
      'api-version': '1.0',
      'language': 'it-IT',
      'countrySet': 'IT',
      'query': addressInput,
      'limit': '1'
    });

    const response = await fetch(
      `https://atlas.microsoft.com/search/address/json?${searchParams.toString()}`,
      {
        headers: {
          'Accept': 'application/json'
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Geocoding failed: ${response.statusText}`);
    }

    const data = await response.json();
    console.log("Geocoding response:", data);

    if (data.results && data.results.length > 0) {
      const result = data.results[0];
      const coordinates = [result.position.lon, result.position.lat];
      const address = result.address.freeformAddress;

      console.log("Found coordinates:", coordinates, "for address:", address);

      document.getElementById("latitude").textContent = result.position.lat.toFixed(6);
      document.getElementById("longitude").textContent = result.position.lon.toFixed(6);

      addMarker(coordinates, address);
      map.setCamera({
        center: coordinates,
        zoom: 15,
        type: "fly",
      });
    } else {
      console.log("No coordinates found");
      document.getElementById("latitude").textContent = "-";
      document.getElementById("longitude").textContent = "-";
      showError("No coordinates found for this address");
    }
  } catch (error) {
    console.error("Conversion error:", error);
    showError("Failed to convert address to coordinates: " + error.message);
  } finally {
    toggleLoading(false);
  }
}

// Convert coordinates to address
async function convertCoordsToAddress() {
  const lat = parseFloat(document.getElementById("latInput").value);
  const lng = parseFloat(document.getElementById("lngInput").value);

  if (isNaN(lat) || isNaN(lng)) {
    showError("Please enter valid coordinates");
    return;
  }

  if (!subscriptionKey) {
    showError("Map not initialized properly. Please refresh the page.");
    return;
  }

  try {
    toggleLoading(true);
    console.log("Converting coordinates to address:", lat, lng);

    const searchParams = new URLSearchParams({
      'subscription-key': subscriptionKey,
      'api-version': '1.0',
      'language': 'it-IT',
      'number': '1'
    });

    const coordinates = `${lat},${lng}`;

    const response = await fetch(
      `https://atlas.microsoft.com/search/address/reverse/json?${searchParams.toString()}&query=${coordinates}`,
      {
        headers: {
          'Accept': 'application/json'
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Reverse geocoding failed: ${response.statusText}`);
    }

    const data = await response.json();
    console.log("Reverse geocoding response:", data);

    if (data.addresses && data.addresses.length > 0) {
      const result = data.addresses[0];
      const address = result.address.freeformAddress;

      console.log("Found address:", address);

      document.getElementById("addressResult").innerHTML = address;
      addMarker([lng, lat], address);
      map.setCamera({
        center: [lng, lat],
        zoom: 15,
        type: "fly",
      });
    } else {
      console.log("No address found");
      document.getElementById("addressResult").innerHTML = "No address found";
      showError("No address found for these coordinates");
    }
  } catch (error) {
    console.error("Reverse geocoding error:", error);
    showError("Failed to convert coordinates to address: " + error.message);
  } finally {
    toggleLoading(false);
  }
}

// Add marker to map
function addMarker(coordinates, title) {
  if (datasource) {
    datasource.clear();
    const point = new atlas.data.Feature(new atlas.data.Point(coordinates), {
      title: title,
    });

    datasource.add(point);
    updateCurrentMarker(coordinates, title);
  }
}

// Update current marker display
function updateCurrentMarker(coordinates, address) {
  document.getElementById("currentMarker").innerHTML = `
    <div>Location: ${address}</div>
    <div>Coordinates: ${coordinates[1].toFixed(6)}, ${coordinates[0].toFixed(6)}</div>
  `;
}

// Clear all markers
function clearMarkers() {
  if (datasource) {
    datasource.clear();
    document.getElementById("currentMarker").innerHTML = "No marker placed";
    document.getElementById("searchResults").innerHTML = "";
    document.getElementById("addressResult").innerHTML = "";
    document.getElementById("latitude").textContent = "-";
    document.getElementById("longitude").textContent = "-";
  }
}

// Initialize map on DOM content loaded
document.addEventListener("DOMContentLoaded", initializeMap);

// Handle window resize events
window.addEventListener("resize", () => {
  if (map) {
    map.resize();
  }
});
