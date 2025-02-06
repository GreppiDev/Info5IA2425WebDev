let viewer;
let currentMarker;
let currentMarkerClickHandler;

// Function to fetch the Cesium Ion token from the server
async function getCesiumIonToken() {
  try {
    const response = await fetch("/api/maps/cesium-ion-token");
    if (!response.ok) {
      throw new Error("Failed to fetch Cesium Ion token");
    }
    const token = await response.json();
    return token.token;
  } catch (error) {
    console.error("Error fetching Cesium Ion token:", error);
    throw error;
  }
}

// Initialize the Cesium viewer
async function initializeMap() {
  try {
    // Get the Cesium Ion token before initializing the map
    const token = await getCesiumIonToken();

    // Set Cesium Ion token
    Cesium.Ion.defaultAccessToken = token;
    // Create imagery provider view models
    const imageryViewModels = [];

    // OpenStreetMap layer
    imageryViewModels.push(
      new Cesium.ProviderViewModel({
        name: "Open Street Map",
        iconUrl: Cesium.buildModuleUrl(
          "Widgets/Images/ImageryProviders/openStreetMap.png"
        ),
        tooltip: "OpenStreetMap",
        category: "Imaginary layers",
        creationFunction: function () {
          return new Cesium.OpenStreetMapImageryProvider({
            url: "https://tile.openstreetmap.org/",
          });
        },
      })
    );

    // ESRI Satellite layer
    imageryViewModels.push(
      new Cesium.ProviderViewModel({
        name: "ESRI World Imagery",
        iconUrl: Cesium.buildModuleUrl(
          "Widgets/Images/ImageryProviders/naturalEarthII.png"
        ),
        tooltip: "ESRI World Imagery",
        category: "Imaginary layers",
        creationFunction: function () {
          return new Cesium.UrlTemplateImageryProvider({
            url: "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
            maximumLevel: 19,
          });
        },
      })
    );

    // Create terrain provider view models
    const terrainViewModels = [];

    // WGS84 Ellipsoid
    terrainViewModels.push(
      new Cesium.ProviderViewModel({
        name: "WGS84 Ellipsoid",
        iconUrl: Cesium.buildModuleUrl(
          "Widgets/Images/TerrainProviders/Ellipsoid.png"
        ),
        tooltip: "WGS84 standard terrain",
        category: "Terrain",
        creationFunction: function () {
          return new Cesium.EllipsoidTerrainProvider();
        },
      })
    );

    // Cesium World Terrain
    terrainViewModels.push(
      new Cesium.ProviderViewModel({
        name: "Cesium World Terrain",
        iconUrl: Cesium.buildModuleUrl(
          "Widgets/Images/TerrainProviders/CesiumWorldTerrain.png"
        ),
        tooltip: "High-resolution global terrain",
        category: "Terrain",
        creationFunction: async function () {
          try {
            return await Cesium.createWorldTerrainAsync({
              requestWaterMask: true,
              requestVertexNormals: true,
            });
          } catch (error) {
            console.error("Error creating terrain provider:", error);
            return new Cesium.EllipsoidTerrainProvider();
          }
        },
      })
    );

    // Initialize Cesium viewer with customized base layer picker
    viewer = new Cesium.Viewer("cesiumContainer", {
      baseLayerPicker: true,
      geocoder: false,
      homeButton: false,
      sceneModePicker: false,
      navigationHelpButton: false,
      animation: false,
      timeline: false,
      fullscreenButton: false,
      infoBox: false,
      imageryProviderViewModels: imageryViewModels,
      selectedImageryProviderViewModel: imageryViewModels[1], // Select ESRI by default
      terrainProviderViewModels: terrainViewModels,
      selectedTerrainProviderViewModel: terrainViewModels[1], // Cesium World Terrain by default
    });

    // Remove credits display
    //viewer.scene.globe.enableLighting = false;
    //viewer.cesiumWidget.creditContainer.style.display = "none";

    // Set initial camera position (Istituto Alessandro Greppi di Monticello Brianza)
    viewer.camera.setView({
      destination: Cesium.Cartesian3.fromDegrees(9.306995, 45.698205, 1000),
      orientation: {
        heading: Cesium.Math.toRadians(0),
        pitch: Cesium.Math.toRadians(-45),
        roll: 0,
      },
    });

    // Initialize controls
    initControls();

    // Disable default mouse wheel zoom
    viewer.scene.screenSpaceCameraController.zoomEventTypes = [];

    // Add custom mouse wheel zoom handling
    const screenSpaceEventHandler = new Cesium.ScreenSpaceEventHandler(
      viewer.scene.canvas
    );
    screenSpaceEventHandler.setInputAction((movement) => {
      const cameraPosition = viewer.camera.positionCartographic;
      const terrainHeight = viewer.scene.globe.getHeight(cameraPosition);
      const distanceFromSurface = cameraPosition.height - (terrainHeight || 0);

      // Zoom in or out based on wheel direction
      const zoomAmount = movement * (distanceFromSurface * 0.0002);

      // If zooming in and too close to surface, prevent zoom
      if (zoomAmount > 0 && distanceFromSurface <= 100) {
        return;
      }

      viewer.camera.zoomIn(zoomAmount);
    }, Cesium.ScreenSpaceEventType.WHEEL);
  } catch (error) {
    console.error("Error initializing map:", error);
    showAlert("Error initializing map. Please try again later.");
  }
}

function showAlert(message) {
  const errorMessage = document.getElementById("errorMessage");
  if (!errorMessage) {
    console.error("Error message element not found");
    return;
  }
  console.error(message);
  errorMessage.textContent = message;
  errorMessage.style.display = "block";
  setTimeout(() => {
    errorMessage.style.opacity = "0";
    setTimeout(() => {
      errorMessage.style.display = "none";
      errorMessage.style.opacity = "1";
    }, 300);
  }, 5000);
}

function initControls() {
  // Address search
  document.getElementById("searchAddress").addEventListener("click", () => {
    const address = document.getElementById("addressInput").value;
    if (address) {
      searchAddress(address);
    }
  });

  // Address to coordinates
  document.getElementById("convertAddress").addEventListener("click", () => {
    const address = document.getElementById("addressInput").value;
    if (address) {
      convertAddressToCoordinates(address);
    }
  });

  // Camera controls
  document.getElementById("zoomIn").addEventListener("click", () => {
    const cameraPosition = viewer.camera.positionCartographic;
    const terrainHeight = viewer.scene.globe.getHeight(cameraPosition);
    const distanceFromSurface = cameraPosition.height - (terrainHeight || 0);

    if (distanceFromSurface > 100) {
      viewer.camera.moveForward(distanceFromSurface * 0.2);
    }
  });

  document.getElementById("zoomOut").addEventListener("click", () => {
    const height = viewer.camera.positionCartographic.height;
    viewer.camera.moveBackward(height * 0.2);
  });

  document.getElementById("pitchUp").addEventListener("click", () => {
    const pitch = viewer.camera.pitch;
    const heading = viewer.camera.heading;
    const position = viewer.camera.position;
    viewer.camera.setView({
      destination: position,
      orientation: {
        heading: heading,
        pitch: pitch + Cesium.Math.toRadians(10),
        roll: 0,
      },
    });
  });

  document.getElementById("pitchDown").addEventListener("click", () => {
    const pitch = viewer.camera.pitch;
    const heading = viewer.camera.heading;
    const position = viewer.camera.position;
    viewer.camera.setView({
      destination: position,
      orientation: {
        heading: heading,
        pitch: pitch - Cesium.Math.toRadians(10),
        roll: 0,
      },
    });
  });

  document.getElementById("panLeft").addEventListener("click", () => {
    const pitch = viewer.camera.pitch;
    const heading = viewer.camera.heading;
    const position = viewer.camera.position;
    viewer.camera.setView({
      destination: position,
      orientation: {
        heading: heading - Cesium.Math.toRadians(10),
        pitch: pitch,
        roll: 0,
      },
    });
  });

  document.getElementById("panRight").addEventListener("click", () => {
    const pitch = viewer.camera.pitch;
    const heading = viewer.camera.heading;
    const position = viewer.camera.position;
    viewer.camera.setView({
      destination: position,
      orientation: {
        heading: heading + Cesium.Math.toRadians(10),
        pitch: pitch,
        roll: 0,
      },
    });
  });
}

function searchAddress(address) {
  fetch(
    `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(
      address
    )}&limit=1`,
    {
      headers: {
        "User-Agent": "MapApp/1.0",
      },
    }
  )
    .then((response) => response.json())
    .then((data) => {
      if (!data || data.length === 0) {
        showAlert("Nessun indirizzo trovato per questa posizione");
        document.getElementById("coordinatesOutput").innerText =
          "Indirizzo non trovato";
        return;
      }
      const lat = parseFloat(data[0].lat);
      const lon = parseFloat(data[0].lon);

      const destination = Cesium.Cartesian3.fromDegrees(lon, lat - 0.005, 1000);

      viewer.camera.flyTo({
        destination: destination,
        orientation: {
          heading: Cesium.Math.toRadians(0),
          pitch: Cesium.Math.toRadians(-45),
          roll: 0,
        },
        duration: 2,
      });

      addMarker(lon, lat);
      document.getElementById(
        "coordinatesOutput"
      ).innerText = `Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}`;
    })
    .catch((error) => {
      console.error("Error searching address:", error);
      document.getElementById("coordinatesOutput").innerText =
        "Errore nella ricerca";
    });
}

function convertAddressToCoordinates(address) {
  fetch(
    `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(
      address
    )}&limit=1`,
    {
      headers: {
        "User-Agent": "MapApp/1.0",
      },
    }
  )
    .then((response) => response.json())
    .then((data) => {
      if (!data || data.length === 0) {
        showAlert("Nessun indirizzo trovato per questa posizione");
        document.getElementById("coordinatesOutput").innerText =
          "Indirizzo non trovato";
        return;
      }
      const lat = parseFloat(data[0].lat);
      const lon = parseFloat(data[0].lon);
      document.getElementById(
        "coordinatesOutput"
      ).innerText = `Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}`;

      addMarker(lon, lat);

      setTimeout(() => {
        if (currentMarker) {
          viewer.entities.remove(currentMarker);
          currentMarker = null;
          document.getElementById("currentMarker").innerHTML = `
                    <h3>Marker Corrente</h3>
                    <div>Nessun marker inserito</div>
                    <div class="control-buttons" style="margin-top: 10px; display: block;">
                        <button id="removeMarker" style="width: 100%">Rimuovi marker</button>
                    </div>
                `;
        }
      }, 5000);
    })
    .catch((error) => {
      console.error("Error converting address:", error);
      document.getElementById("coordinatesOutput").innerText =
        "Errore nella conversione";
    });
}

function reverseGeocode(lon, lat) {
  return fetch(
    `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`,
    {
      headers: {
        "User-Agent": "MapApp/1.0",
      },
    }
  )
    .then((response) => response.json())
    .then((data) => {
      if (data.error) {
        throw new Error(data.error);
      }
      return data.display_name || "Indirizzo non disponibile";
    });
}

function addMarker(lon, lat) {
  if (currentMarker) {
    viewer.entities.remove(currentMarker);
  }

  const markerSvg = `data:image/svg+xml;base64,${btoa(`
				<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="32" height="32">
						<path fill="#dc3545" d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
				</svg>
		`)}`;

  currentMarker = viewer.entities.add({
    position: Cesium.Cartesian3.fromDegrees(lon, lat),
    billboard: {
      image: markerSvg,
      verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
      heightReference: Cesium.HeightReference.CLAMP_TO_GROUND,
      scale: 1.0,
    },
  });

  currentMarker.markerLocation = { lon, lat };
// Remove previous event listener if it exists
if (currentMarkerClickHandler) {
  viewer.selectedEntityChanged.removeEventListener(currentMarkerClickHandler);
}

// Add new event listener
currentMarkerClickHandler = (selectedEntity) => {
  if (selectedEntity === currentMarker) {
    reverseGeocode(lon, lat)
      .then((address) => {
        document.getElementById("currentMarker").innerHTML = `
<h3>Marker Corrente</h3>
<div>${address}</div>
<div class="control-buttons" style="margin-top: 10px; display: block;">
<button id="removeMarker" style="width: 100%">Rimuovi marker</button>
</div>
`;
        const button = document.getElementById("removeMarker");
        if (button) {
          button.addEventListener("click", function removeMarkerHandler() {
            if (currentMarker && viewer.entities.contains(currentMarker)) {
              try {
                viewer.entities.remove(currentMarker);
                currentMarker = null;
                document.getElementById("currentMarker").innerHTML = `
<h3>Marker Corrente</h3>
<div>Nessun marker inserito</div>
<div class="control-buttons" style="margin-top: 10px; display: block;">
<button id="removeMarker" style="width: 100%">Rimuovi marker</button>
</div>
`;
              } catch (error) {
                console.error("Error removing marker:", error);
              }
            }
          });
        }
      })
      .catch((error) => {
        console.error("Error getting address:", error);
        document.getElementById("currentMarker").innerHTML = `
<h3>Marker Corrente</h3>
<div>Indirizzo non disponibile</div>
<div class="control-buttons" style="margin-top: 10px; display: block;">
<button id="removeMarker" style="width: 100%">Rimuovi marker</button>
</div>
`;
      });
  }
};

// Register the event listener
viewer.selectedEntityChanged.addEventListener(currentMarkerClickHandler);

  // Initial display with coordinates
  document.getElementById("currentMarker").innerHTML = `
				<h3>Marker Corrente</h3>
				<div>Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}</div>
				<div class="control-buttons" style="margin-top: 10px; display: block;">
						<button id="removeMarker" style="width: 100%">Rimuovi marker</button>
				</div>
		`;

  const button = document.getElementById("removeMarker");
  if (button) {
    button.addEventListener("click", function () {
      if (currentMarker && viewer.entities.contains(currentMarker)) {
        try {
          viewer.entities.remove(currentMarker);
          currentMarker = null;
          document.getElementById("currentMarker").innerHTML = `
												<h3>Marker Corrente</h3>
												<div>Nessun marker inserito</div>
												<div class="control-buttons" style="margin-top: 10px; display: block;">
														<button id="removeMarker" style="width: 100%">Rimuovi marker</button>
												</div>
										`;
        } catch (error) {
          console.error("Error removing marker:", error);
        }
      }
    });
  }
}

// Initialize the map
initializeMap();
