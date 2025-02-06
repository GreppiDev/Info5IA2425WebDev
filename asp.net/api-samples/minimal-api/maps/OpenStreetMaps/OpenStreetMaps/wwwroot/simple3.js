// Function to get array of tile URLs for a 2x2 grid
function getTileURLs(lat, lon, zoom) {
	// Convert lat/lon to tile coordinates for center tile
	const x = Math.floor(((lon + 180) / 360) * Math.pow(2, zoom));
	const y = Math.floor((1 - Math.log(Math.tan(lat * Math.PI / 180) + 1 / Math.cos(lat * Math.PI / 180)) / Math.PI) / 2 * Math.pow(2, zoom));
	
	// Create 2x2 grid of tiles around the center point
	const tiles = [
		// Top row
		`https://tile.openstreetmap.org/${zoom}/${x}/${y}.png`,
		`https://tile.openstreetmap.org/${zoom}/${x+1}/${y}.png`,
		// Bottom row
		`https://tile.openstreetmap.org/${zoom}/${x}/${y+1}.png`,
		`https://tile.openstreetmap.org/${zoom}/${x+1}/${y+1}.png`
	];

	// Create container div with CSS grid
	const container = document.createElement('div');
	container.style.display = 'grid';
	container.style.gridTemplateColumns = 'repeat(2, 256px)';
	container.style.width = '512px';
	container.style.height = '512px';
	container.className = 'static-map-container';

	// Create and add image elements
	tiles.forEach((url, index) => {
		const img = document.createElement('img');
		img.src = url;
		img.style.width = '256px';
		img.style.height = '256px';
		img.style.display = 'block';
		container.appendChild(img);
	});

	return container;
}

// Set the static map
document.addEventListener('DOMContentLoaded', function() {
    const container = getTileURLs(41.8902, 12.4922, 17);
    const mapContainer = document.getElementById('static-map-1');
    mapContainer.parentNode.replaceChild(container, mapContainer);
});

