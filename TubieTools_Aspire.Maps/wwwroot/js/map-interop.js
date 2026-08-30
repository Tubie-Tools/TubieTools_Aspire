/**
 * Map Interoperability Functions for Blazor Route Planner
 * Handles map rendering, route visualization, and marker management
 */

let mapInstance = null;
let routePolyline = null;
let startMarker = null;
let endMarker = null;
let waypointMarkers = [];

/**
 * Initializes Leaflet map (requires Leaflet library)
 */
function initializeMap(containerId = 'mapContainer') {
    if (mapInstance !== null) {
        return mapInstance;
    }

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container ${containerId} not found`);
        return null;
    }

    // Initialize Leaflet map
    mapInstance = L.map(containerId).setView([40.7128, -74.0060], 11);

    // Add tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19,
    }).addTo(mapInstance);

    // Add click handler
    mapInstance.on('click', function(e) {
        console.log(`Map clicked at ${e.latlng.lat}, ${e.latlng.lng}`);
    });

    return mapInstance;
}

/**
 * Renders a route on the map
 * @param {Array} coordinates Array of [lat, lon] coordinate pairs
 * @param {string} color Route line color (default: blue)
 */
function renderRoute(coordinates, color = '#0d6efd') {
    if (!mapInstance) {
        initializeMap();
    }

    if (!coordinates || coordinates.length === 0) {
        console.warn('No coordinates to render');
        return;
    }

    // Clear existing route
    if (routePolyline) {
        mapInstance.removeLayer(routePolyline);
    }

    // Convert coordinates to Leaflet format [lat, lon]
    const latLngs = coordinates.map(coord => [coord.lat, coord.lon]);

    // Create and add polyline
    routePolyline = L.polyline(latLngs, {
        color: color,
        weight: 4,
        opacity: 0.8,
        lineCap: 'round',
        lineJoin: 'round',
        dashArray: null,
        className: 'route-line'
    }).addTo(mapInstance);

    // Add start marker
    if (latLngs.length > 0) {
        clearMarkers();
        startMarker = L.circleMarker(latLngs[0], {
            radius: 8,
            fillColor: '#28a745',
            color: '#fff',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8,
            className: 'start-marker'
        }).addTo(mapInstance)
            .bindPopup('Start Location')
            .openPopup();
    }

    // Add end marker
    if (latLngs.length > 1) {
        endMarker = L.circleMarker(latLngs[latLngs.length - 1], {
            radius: 8,
            fillColor: '#dc3545',
            color: '#fff',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8,
            className: 'end-marker'
        }).addTo(mapInstance)
            .bindPopup('End Location')
            .openPopup();
    }

    // Add waypoint markers
    for (let i = 1; i < latLngs.length - 1; i++) {
        const marker = L.circleMarker(latLngs[i], {
            radius: 6,
            fillColor: '#fd7e14',
            color: '#fff',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8,
            className: 'waypoint-marker'
        }).addTo(mapInstance)
            .bindPopup(`Waypoint ${i}`);

        waypointMarkers.push(marker);
    }

    // Fit bounds to route
    const bounds = L.latLngBounds(latLngs);
    mapInstance.fitBounds(bounds, { padding: [50, 50] });
}

/**
 * Adds location markers to the map
 * @param {Array} locations Array of location objects {name, latitude, longitude, id}
 */
function addLocationMarkers(locations) {
    if (!mapInstance) {
        initializeMap();
    }

    if (!locations || !Array.isArray(locations)) {
        console.error('Invalid locations array');
        return;
    }

    locations.forEach(location => {
        L.marker([location.latitude, location.longitude], {
            title: location.name,
            icon: L.icon({
                iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34],
                shadowSize: [41, 41]
            })
        }).addTo(mapInstance)
            .bindPopup(`<strong>${location.name}</strong><br/>ID: ${location.id}`);
    });
}

/**
 * Clears all markers from the map
 */
function clearMarkers() {
    if (startMarker) {
        mapInstance.removeLayer(startMarker);
        startMarker = null;
    }
    if (endMarker) {
        mapInstance.removeLayer(endMarker);
        endMarker = null;
    }
    waypointMarkers.forEach(marker => {
        mapInstance.removeLayer(marker);
    });
    waypointMarkers = [];
}

/**
 * Clears the route from the map
 */
function clearRoute() {
    if (routePolyline) {
        mapInstance.removeLayer(routePolyline);
        routePolyline = null;
    }
    clearMarkers();
}

/**
 * Gets the current map center
 * @returns {Object} {lat, lon}
 */
function getMapCenter() {
    if (!mapInstance) return null;
    const center = mapInstance.getCenter();
    return {
        lat: center.lat,
        lon: center.lng
    };
}

/**
 * Sets map center and zoom
 * @param {number} lat Latitude
 * @param {number} lon Longitude
 * @param {number} zoom Zoom level (default: 12)
 */
function setMapCenter(lat, lon, zoom = 12) {
    if (!mapInstance) {
        initializeMap();
    }
    mapInstance.setView([lat, lon], zoom);
}

/**
 * Gets map bounds
 * @returns {Object} {north, south, east, west}
 */
function getMapBounds() {
    if (!mapInstance) return null;
    const bounds = mapInstance.getBounds();
    return {
        north: bounds.getNorth(),
        south: bounds.getSouth(),
        east: bounds.getEast(),
        west: bounds.getWest()
    };
}

/**
 * Adds a circle/radius overlay to the map
 * @param {number} lat Center latitude
 * @param {number} lon Center longitude
 * @param {number} radiusMeters Radius in meters
 * @param {string} color Circle color
 */
function addRadiusCircle(lat, lon, radiusMeters, color = '#0d6efd') {
    if (!mapInstance) {
        initializeMap();
    }

    return L.circle([lat, lon], {
        color: color,
        fillColor: color,
        fillOpacity: 0.1,
        weight: 2,
        radius: radiusMeters
    }).addTo(mapInstance);
}

/**
 * Downloads route data as JSON file
 * @param {string} jsonData JSON data to download
 * @param {string} filename Filename for download
 */
function downloadJSON(jsonData, filename) {
    const dataStr = typeof jsonData === 'string' ? jsonData : JSON.stringify(jsonData);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

/**
 * Exports map view as image
 * Requires leaflet-image library
 */
function exportMapAsImage(callback) {
    if (!window.leafletImage) {
        console.error('leaflet-image library not loaded');
        return;
    }

    leafletImage(mapInstance, function(err, canvas) {
        if (err) {
            console.error('Error exporting map:', err);
            return;
        }

        const image = canvas.toDataURL('image/png');
        if (callback) {
            callback(image);
        }

        // Download image
        const link = document.createElement('a');
        link.href = image;
        link.download = 'route-map.png';
        link.click();
    });
}

/**
 * Enables map terrain/satellite view
 * @param {string} type 'street' | 'satellite' | 'terrain'
 */
function setMapType(type) {
    if (!mapInstance) return;

    // Remove existing layers
    mapInstance.eachLayer(layer => {
        if (layer instanceof L.TileLayer && layer !== routePolyline) {
            mapInstance.removeLayer(layer);
        }
    });

    // Add new tile layer based on type
    let tileLayer;
    switch (type) {
        case 'satellite':
            tileLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
                attribution: '© Tiles © Esri',
                maxZoom: 18
            });
            break;
        case 'terrain':
            tileLayer = L.tileLayer('https://tiles.stadiamaps.com/tiles/stamen_terrain/{z}/{x}/{y}.png', {
                attribution: '© Stadia Maps © Stamen Design',
                maxZoom: 18
            });
            break;
        default:
            tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors',
                maxZoom: 19
            });
    }

    tileLayer.addTo(mapInstance);
}

/**
 * Measures distance between two points
 * @param {Array} point1 [lat, lon]
 * @param {Array} point2 [lat, lon]
 * @returns {number} Distance in kilometers
 */
function measureDistance(point1, point2) {
    const R = 6371; // Earth's radius in km
    const dLat = (point2[0] - point1[0]) * Math.PI / 180;
    const dLon = (point2[1] - point1[1]) * Math.PI / 180;
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(point1[0] * Math.PI / 180) * Math.cos(point2[0] * Math.PI / 180) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
}

/**
 * Creates a GeoJSON layer from GeoJSON data
 * @param {Object} geoJsonData GeoJSON object
 * @param {Function} onEachFeature Optional callback for each feature
 */
function addGeoJSONLayer(geoJsonData, onEachFeature) {
    if (!mapInstance) {
        initializeMap();
    }

    const geoJsonLayer = L.geoJSON(geoJsonData, {
        onEachFeature: onEachFeature || function(feature, layer) {
            if (feature.properties) {
                let popupContent = '<div>';
                for (let key in feature.properties) {
                    popupContent += `<strong>${key}:</strong> ${feature.properties[key]}<br/>`;
                }
                popupContent += '</div>';
                layer.bindPopup(popupContent);
            }
        },
        pointToLayer: function(feature, latlng) {
            return L.circleMarker(latlng, {
                radius: 6,
                fillColor: '#0d6efd',
                color: '#fff',
                weight: 2,
                opacity: 1,
                fillOpacity: 0.8
            });
        },
        style: function(feature) {
            return {
                color: feature.properties.color || '#0d6efd',
                weight: 4,
                opacity: 0.8
            };
        }
    }).addTo(mapInstance);

    return geoJsonLayer;
}

/**
 * Gets route polyline as GeoJSON
 * @returns {Object} GeoJSON LineString
 */
function getRouteAsGeoJSON() {
    if (!routePolyline) return null;

    const coordinates = routePolyline.getLatLngs().map(latlng => [latlng.lng, latlng.lat]);
    return {
        type: 'LineString',
        coordinates: coordinates
    };
}

// Auto-initialize map when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    const mapContainer = document.getElementById('mapContainer');
    if (mapContainer && !mapInstance) {
        setTimeout(() => {
            initializeMap('mapContainer');
        }, 100);
    }
});

// Export functions for global access
window.RouteMapInterop = {
    initializeMap,
    renderRoute,
    addLocationMarkers,
    clearMarkers,
    clearRoute,
    getMapCenter,
    setMapCenter,
    getMapBounds,
    addRadiusCircle,
    downloadJSON,
    exportMapAsImage,
    setMapType,
    measureDistance,
    addGeoJSONLayer,
    getRouteAsGeoJSON
};
