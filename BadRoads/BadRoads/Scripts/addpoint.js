$(document).ready(function () { Initialize(); AlreadySetLocation(); });
var myLatlng = new google.maps.LatLng(48.466601, 35.018155);              // center of map
var map;                                                                  // map
var geocoder;                                                             // object of class Geocoder
var markers = new Array();

var searchMarker;                                                         // marker for street searching

var imageSearchMarker = "../../Images/newmarker.png";                     // image for marker for street searching

function SetPoints() {                                                    // method to set all points on map
    var masPoints = document.getElementsByClassName("points");            // get coordiantes of all points from html
    var imageMarker = "../../Images/marker.png";                          // image for markers
    if (masPoints.length > 0) {
        for (var x = 0; x < masPoints.length; x++) {
            var la = $(masPoints[x]).data('latitude');
            la = la.replace(",", ".");
            var ln = $(masPoints[x]).data('longitude');
            ln = ln.replace(",", ".");
            markers[x] = new google.maps.Marker({                         // create marker
                position: new google.maps.LatLng(la, ln),
                map: map,
                icon: imageMarker,
                title: $(masPoints[x]).data('adress')
            });
            markers[x].idPoint = $(masPoints[x]).data('id');                         // set marker's property with point's ID
            google.maps.event.addListener(markers[x], 'click', function () {         // sign marker on event "click"
                window.location.assign("../../Point/PointInfo/" + this.idPoint);     // jump in the action of detailed view of point
            });
        }

        var markerClusterer = new MarkerClusterer(map, markers,                      // create object MarkerClusterer to group markers on map
    {                                                                                // settings to group markers
        maxZoom: 13,
        gridSize: 50,
        styles: null
    });
    }

}
function Initialize() {
    geocoder = new google.maps.Geocoder();                                           //   create Geocoder object
    var mapOptions = {                                                               // set options for map 
        center: myLatlng,
        zoom: 12,
        mapTypeId: google.maps.MapTypeId.ROADMAP                                     //  type of map. ROADMAP - for roads
    };
    map = new google.maps.Map(document.getElementById("map"), mapOptions);           //  create map 
    SetPoints();                                                                     //  call the method to set all points on map


    // Initialization of marker for search
    //02.04.2015 Konovalenko Anton
    searchMarker = new google.maps.Marker({
        map: map,
        icon: imageSearchMarker,
        draggable: true,
    });



    google.maps.event.addListener(map, 'click', function (data) {                            // click on map
        var latlng = new google.maps.LatLng(data.latLng.lat(), data.latLng.lng());
        searchMarker.setPosition(latlng);
        $("#latitude").val(data.latLng.lat());                                               // add latitude on form
        $("#longitude").val(data.latLng.lng());                                              // add longitude on form

        geocoder.geocode({ 'latLng': latlng }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                $("#adresset").val(results[0].address_components[1].long_name);                 //  add adress on form (street or district)
                $("#subm").removeAttr("disabled");                                              // button to send form set enabled
            } else {
                alert('Geocode was not successful for the following reason: ' + status);        // message if geocoding failed
            }
        });


    });
}

function AlreadySetLocation() {                         //function that set marker for new point if it's location have been already set 
    var stringLocation = $("#stringforMap").data("str");
    if (stringLocation != "") {                                            // execute only if coordinates have been sent from action
        var arr = stringLocation.split("-");
        var latlng = new google.maps.LatLng(arr[0], arr[1]);
        searchMarker.setPosition(latlng);                                  // set marker in the same location as on main map
        map.setCenter(latlng);                                             // set the center of map on this location 
        $("#latitude").val(arr[0]);                                        // add latitude on form
        $("#longitude").val(arr[1]);                                       // add longitude on form
        geocoder.geocode({ 'latLng': latlng }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                $("#adresset").val(results[0].address_components[1].long_name);                  //  add adress on form (street or district)
                $("#subm").removeAttr("disabled");                                               //  button to send form set enabled
            } else { 
                alert('Geocode was not successful for the following reason: ' + status);        //  message if geocoding failed
            }
        });
    }
}
