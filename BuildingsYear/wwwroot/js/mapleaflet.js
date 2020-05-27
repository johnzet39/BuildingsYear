var project1 = '+proj=tmerc +lat_0=0 +lon_0=21.45 +k=1 +x_0=1250122.7 +y_0=-5711030.0 +ellps=krass +units=m +no_defs';

//var map = L.map('map_l', { maxZoom: 18, minZoom: 11}).setView([54.7, 20.45], 12);
var map = L.map('map_l', { attributionControl: false, maxZoom: 18, minZoom: 12, zoomControl: false }).setView([54.71, 20.51], 13);
L.control.zoom({position: 'bottomleft'}).addTo(map);
L.DomUtil.addClass(map._container, 'crosshair-cursor-enabled');
//var osm = new L.TileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', { maxZoom: 19 }).addTo(map);

var tiles = new L.TileLayer(window.origin + '/api/map/{z}/{x}/{y}.png').addTo(map);
//var tiles = new L.TileLayer('http://localhost:50201/api/map/{z}/{x}/{y}.png').addTo(map);

var selectedStyle = {
    "color": "#fded0d",
    "weight": 2,
    "opacity": 0.9
};

var groupSelectedPolygon = new L.featureGroup().addTo(map);
var selectedPolygon = L.geoJSON(null, { style: selectedStyle }).addTo(groupSelectedPolygon);

setDefaultMapClickEvent();

function setDefaultMapClickEvent() {
    map.on('click', defaultMapClick);
};

function defaultMapClick(e) {
    url = 'api/map/getinfo/' + e.latlng.lng + '/' + e.latlng.lat;
    $.get(url).done(function (data) {
        if (data.error) {
            $("#info-panel").addClass('info-hidden');
            selectedPolygon.clearLayers();
        }
        else {
            selectedPolygon.clearLayers();
            if (data.featureInfo != null) {
                var string_year = JSON.parse(data.featureInfo).year;
                var string_address = JSON.parse(data.featureInfo).address;
                if (string_year.length > 0 && string_address.length > 0) {
                    document.getElementById("info-year").innerHTML = string_year;
                    document.getElementById("info-address").innerHTML = string_address;
                    $("#info-panel").removeClass('info-hidden');

                    selectFeature(JSON.parse(data.featureInfo).geom);
                }
                else {
                    $("#info-panel").addClass('info-hidden');
                }
            }
        }
    });
}

function selectFeature(geometry) {
    var wicket = new Wkt.Wkt();
    wicket.read(geometry);
    var polygon = wicket.toJson();
    selectedPolygon.addData(polygon);
}

function setEditBuildClickEvent() {
    map.off('click', defaultMapClick);
    map.on('click', editMapClick);
}

function unsetEditBuildClickEvent() {
    map.on('click', defaultMapClick);
    map.off('click', editMapClick);
}

function editMapClick(e) {
    console.log(e.latlng.lng + ', ' + e.latlng.lat);
    toggleEditMode();
}

function toggleEditMode() {
    var btn = $('button[data-toggle="showOnMap"]');
    if (btn.hasClass('btn-edit-on')) {
        btn.removeClass('btn-edit-on');
        btn.text('Внести правку');
        unsetEditBuildClickEvent();
    }
    else {
        btn.addClass('btn-edit-on');
        btn.text('Отмена');
        setEditBuildClickEvent();
    }
}

$('button[data-toggle="edityear"]').click(function (ev) {
    toggleEditMode();
});

$('#btnhide').click(function (event) {
    $('#subtitle').toggle('show');

    if ($(this).hasClass('show')) {
        $(this).removeClass('show');
    }
    else {
        $(this).addClass('show');
    }
});

//UTFGrid

