// Map 1
// Function to get a single tile URL
function getTileURLs(lat, lon, zoom) {
	// Convert lat/lon to tile coordinates
	const x = Math.floor(((lon + 180) / 360) * Math.pow(2, zoom));
	const y = Math.floor(((1 - Math.log(Math.tan((lat * Math.PI) / 180) + 1 / Math.cos((lat * Math.PI) / 180)) / Math.PI) /2) * Math.pow(2, zoom));

	// Create and return a single image element
	const img = document.createElement("img");
	img.src = `https://tile.openstreetmap.org/${zoom}/${x}/${y}.png`;
	img.style.width = "256px";
	img.style.height = "256px";
	img.style.display = "block";
	img.className = "static-map";

	return img;
}

// Set the static map
document.addEventListener('DOMContentLoaded', function() {
		const container = getTileURLs(41.8902, 12.4922, 17);
		const mapContainer = document.getElementById('static-map-1');
		mapContainer.parentNode.replaceChild(container, mapContainer);
});

