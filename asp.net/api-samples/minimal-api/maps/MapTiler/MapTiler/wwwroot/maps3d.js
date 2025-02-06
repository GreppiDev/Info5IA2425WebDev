let viewer;
let currentMarker;
let mapTilerKey;

// Get API key from backend
fetch("/api/maps/map-tiler-key")
.then((response) => response.json())
.then((data) => {
    mapTilerKey = data.key;

    // Initialize Cesium viewer
    viewer = new Cesium.Viewer("cesiumContainer", {
        animation: false,
        baseLayerPicker: false, // Disable the base layer picker
        navigationHelpButton: false,
        sceneModePicker: false,
        homeButton: false,
        geocoder: false,
        fullscreenButton: false,
        infoBox: false,
        timeline: false,
        baseLayer: new Cesium.ImageryLayer(new Cesium.UrlTemplateImageryProvider({
            url: `https://api.maptiler.com/tiles/satellite-v2/{z}/{x}/{y}.jpg?key=${mapTilerKey}`,
            minimumLevel: 0,
            maximumLevel: 20,
            tileWidth: 512,
            tileHeight: 512,
            credit: new Cesium.Credit(
                '<a href="https://www.maptiler.com/copyright/" target="_blank">© MapTiler</a> <a href="https://www.openstreetmap.org/copyright" target="_blank">© OpenStreetMap contributors</a>',
                true
            )
        })),
        terrain: new Cesium.Terrain(Cesium.CesiumTerrainProvider.fromUrl(
            `https://api.maptiler.com/tiles/terrain-quantized-mesh-v2/?key=${mapTilerKey}`, {
                credit: new Cesium.Credit(
                    '<a href="https://www.maptiler.com/copyright/" target="_blank">© MapTiler</a> <a href="https://www.openstreetmap.org/copyright" target="_blank">© OpenStreetMap contributors</a>',
                    true
                ),
                requestVertexNormals: true
            }
        ))
    });

    // Coordinates of Istituto Alessandro Greppi di Monticello Brianza
    const targetLon = 9.306995;
    const targetLat = 45.703205;

    // Set initial view
    const destination = Cesium.Cartesian3.fromDegrees(
        targetLon,
        targetLat - 0.005, // Offset slightly south for better view
        1000 // Initial height at 1000m
    );

    viewer.camera.setView({
        destination: destination,
        orientation: {
            heading: Cesium.Math.toRadians(0), // Looking north
            pitch: Cesium.Math.toRadians(-45), // 45-degree angle looking down
            roll: 0
        }
    });

    // Initialize controls
    initControls();
})
.catch((error) => {
    console.error("Error fetching API key:", error);
});

function showAlert(message) {
    const errorMessage = document.getElementById('errorMessage');
    if (!errorMessage) {
        console.error('Error message element not found');
        return;
    }
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
        const height = viewer.camera.positionCartographic.height;
        viewer.camera.moveForward(height * 0.2);
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
                roll: 0
            }
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
                roll: 0
            }
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
                roll: 0
            }
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
                roll: 0
            }
        });
    });
}

function searchAddress(address) {
    fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1`,
        {
            headers: {
                "User-Agent": "MapApp/1.0"
            }
        }
    )
    .then((response) => response.json())
    .then((data) => {
        if (!data || data.length === 0) {
            showAlert("Nessun indirizzo trovato per questa posizione");
            document.getElementById("coordinatesOutput").innerText = "Indirizzo non trovato";
            return;
        }
        const lat = parseFloat(data[0].lat);
        const lon = parseFloat(data[0].lon);
        
        const destination = Cesium.Cartesian3.fromDegrees(
            lon,
            lat - 0.005,
            1000
        );

        viewer.camera.flyTo({
            destination: destination,
            orientation: {
                heading: Cesium.Math.toRadians(0),
                pitch: Cesium.Math.toRadians(-45),
                roll: 0
            },
            duration: 2
        });

        addMarker(lon, lat);
        document.getElementById("coordinatesOutput").innerText = `Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}`;
    })
    .catch((error) => {
        console.error("Error searching address:", error);
        document.getElementById("coordinatesOutput").innerText = "Errore nella ricerca";
    });
}

function convertAddressToCoordinates(address) {
    fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1`,
        {
            headers: {
                "User-Agent": "MapApp/1.0"
            }
        }
    )
    .then((response) => response.json())
    .then((data) => {
        if (!data || data.length === 0) {
            showAlert("Nessun indirizzo trovato per questa posizione");
            document.getElementById("coordinatesOutput").innerText = "Indirizzo non trovato";
            return;
        }
        const lat = parseFloat(data[0].lat);
        const lon = parseFloat(data[0].lon);
        document.getElementById("coordinatesOutput").innerText = `Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}`;
        
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
        document.getElementById("coordinatesOutput").innerText = "Errore nella conversione";
    });
}

function reverseGeocode(lon, lat) {
    return fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}`,
        {
            headers: {
                "User-Agent": "MapApp/1.0"
            }
        }
    )
    .then(response => response.json())
    .then(data => {
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
            scale: 1.0
        }
    });

    currentMarker.markerLocation = { lon, lat };

    viewer.selectedEntityChanged.addEventListener((selectedEntity) => {
        if (selectedEntity === currentMarker) {
            reverseGeocode(lon, lat)
                .then(address => {
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
                .catch(error => {
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
    });

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
        button.addEventListener("click", function() {
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
