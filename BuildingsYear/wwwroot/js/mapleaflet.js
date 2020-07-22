var project1 = '+proj=tmerc +lat_0=0 +lon_0=21.45 +k=1 +x_0=1250122.7 +y_0=-5711030.0 +ellps=krass +units=m +no_defs';


//var pc_browser = !L.Browser.mobile;
//console.log(pc_browser);

var map = L.map('map_l', { attributionControl: false, maxZoom: 17, minZoom: 12, zoomControl: false, dragging: true, tap: true }).setView([54.71, 20.51], 12);
L.control.zoom({position: 'bottomleft'}).addTo(map);
L.DomUtil.addClass(map._container, 'crosshair-cursor-enabled');
//var osm = new L.TileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', { maxZoom: 19 }).addTo(map);

var tiles = new L.TileLayer(window.origin + '/api/map/{z}/{x}/{y}.png').addTo(map);

var selectedStyle = {
    "color": "#fded0d",
    "weight": 2,
    "opacity": 0.9
};



var groupSelectedPolygon = new L.featureGroup().addTo(map);
var selectedPolygon = L.geoJSON(null, { style: selectedStyle }).addTo(groupSelectedPolygon);
var current_keyid_building = null; //for edit

setDefaultMapClickEvent();

function setDefaultMapClickEvent() {
    map.on('click', defaultMapClick);
};

//Info panel elements
var element_info_year = document.getElementById("info-year");
var element_info_address = document.getElementById("info-address");
var element_info_source = document.getElementById("info-source");
var element_info_klgd_img = document.getElementById("info-klgd-img");
var element_info_klgd_descr = document.getElementById("info-klgd-descr");


function defaultMapClick(e) {
    url = 'api/map/getinfo/' + e.latlng.lng + '/' + e.latlng.lat;
    $.get(url).done(function (data) {
        if (data.error) {
            closeInfoPanel();
        }
        else {
            selectedPolygon.clearLayers();
            if (data.featureInfo != null) {
                selectFeature(JSON.parse(data.featureInfo).geom);

                var json_object = JSON.parse(data.featureInfo);
                
                current_keyid_building = json_object.keyid;

                var string_year = json_object.year.replace('1945', 'до 1945');
                var string_address = json_object.address;
                
                if (string_year.length < 1) {
                    string_year = 'Неизвестно, вы можете внести свои данные';
                }
                if (string_address.length < 1) {
                    string_address = ' - ';
                }
                element_info_year.innerHTML = string_year;
                element_info_address.innerHTML = string_address;
                

                var string_klgd_img_url = json_object.klgd_img_url;
                var string_klgd_descr = json_object.klgd_descr;
                var string_klgd_source = json_object.klgd_source;
                if (string_klgd_source.length > 0 && string_klgd_descr.length > 0) {
                    element_info_source.innerHTML = string_klgd_source;
                    element_info_klgd_img.setAttribute('src', string_klgd_img_url);
                    //document.getElementById("info-klgd-img").setAttribute('src', "c3fb3b9ed7a7c9f117513a82c9b45bb4.jpg");
                    element_info_klgd_descr.innerHTML = string_klgd_descr;
                    $("#building-description").removeClass('hidden');
                }
                else {
                    $("#building-description").addClass('hidden');
                }
                $("#info-panel").removeClass('info-hidden');
                
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

function closeInfoPanel() {
    $("#info-panel").addClass('info-hidden');
    selectedPolygon.clearLayers();
    current_keyid_building = null;

    element_info_year.innerHTML = '';
    element_info_address.innerHTML = '';
    element_info_source.innerHTML = '';
    element_info_klgd_img.removeAttribute('src');
    element_info_klgd_descr.innerHTML = ''; 
}


/************************
*** START info panel
************************/
$('#btn-close-info').click(function (e) {
    closeInfoPanel();
});

$('button[data-toggle="ajax-edit-modal"]').click(function (e) {
    event.preventDefault();
    var url = $(this).data('url');
    $.get(url, { keyid: current_keyid_building }).done(function (data) {
        $('#modal-placeholder').html(data);
        $('#modal-placeholder > .modal').modal('show');
    }).fail(function (jqXHR, textStatus, errorThrown) {
        //alert("Custom error : " + jqXHR.responseText + " Status: " + textStatus + " Http error:" + errorThrown);
        alert(jqXHR.responseText);
    });
});
/************************
*** END info panel
************************/


/************************
*** START hide subtitle
************************/
function hideSubtitle() {
    $('.legend .legend-subtitle').toggleClass('hidden');
    var elem = $('.btn-hide-subtitle .icon-hide');

    if (elem.hasClass('show')) {
        elem.removeClass('show');
    }
    else {
        elem.addClass('show');
    }
}
setTimeout(function () { if (!$('.legend .legend-subtitle').hasClass('hidden')) hideSubtitle(); }, 2000);

$('#btnhide').click(function (event) {
    hideSubtitle();
});
/************************
*** END hide subtitle
************************/


/************************
*** START legend info
************************/
$('.legend-source .info-button').click(function (event) {
    var extra_elem = $('.legend-source .extra-info');

    extra_elem.toggleClass('hidden');
});

/************************
*** END hide subtitle
************************/

//UTFGrid

