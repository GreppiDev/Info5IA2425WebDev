// Map 1: Marker at specific address (Colosseum, Rome)
const map1 = L.map("map1").setView([41.8902, 12.4922], 15);
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
  attribution: "© OpenStreetMap contributors",
}).addTo(map1);

const colosseumMarker = L.marker([41.8902, 12.4922])
  .bindPopup("Piazza del Colosseo, 1, 00184 Roma RM, Italy")
  .addTo(map1);

// Map 2: ESRI Satellite with custom marker and coordinates
const map2 = L.map("map2").setView([41.9022, 12.4539], 16);
L.esri.basemapLayer("Imagery").addTo(map2);



// Add custom marker at St. Peter's Basilica
const vaticanMarker = L.marker([41.9022, 12.4539])
  .bindPopup("St. Peter's Basilica")
  .addTo(map2);

// Update coordinates display on map move
const coordsDisplay = document.getElementById('coordinates2');
map2.on('moveend', function() {
  const center = map2.getCenter();
  coordsDisplay.textContent = `Center coordinates: ${center.lat.toFixed(4)}, ${center.lng.toFixed(4)}`;
});
// Initial coordinates display
const initialCenter = map2.getCenter();
coordsDisplay.textContent = `Center coordinates: ${initialCenter.lat.toFixed(4)}, ${initialCenter.lng.toFixed(4)}`;

// Map 3: Rome Historic Center with GeoJSON
const map3 = L.map("map3").setView([41.9028, 12.4964], 13);
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
  attribution: "© OpenStreetMap contributors",
}).addTo(map3);

// Simplified GeoJSON of Rome's historic center
const historicCenterGeoJSON = {
  type: "Feature",
  properties: {
    name: "Rome Historic Center",
  },
  geometry: {
    type: "Polygon",
    coordinates: [
      [
        [12.4686, 41.9024],
        [12.4831, 41.9059],
        [12.4989, 41.9009],
        [12.501, 41.8929],
        [12.4937, 41.8851],
        [12.4807, 41.8829],
        [12.4712, 41.8859],
        [12.4686, 41.9024],
      ],
    ],
  },
};

try {
  const geoJsonLayer = L.geoJSON(historicCenterGeoJSON, {
    style: {
      color: "#ff7800",
      weight: 3,
      opacity: 0.7,
      fillOpacity: 0.2,
    },
  })
    .bindPopup("Rome Historic Center - UNESCO World Heritage Site")
    .addTo(map3);

  // Fit map to GeoJSON bounds
  map3.fitBounds(geoJsonLayer.getBounds());
} catch (error) {
  console.error("Error rendering GeoJSON:", error);
}
