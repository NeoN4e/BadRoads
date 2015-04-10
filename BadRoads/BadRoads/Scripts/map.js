$(document).ready(function () { AlreadySetLocation();  Initialize(); });
var myLatlng;                                              // center of map
var map;                                                   // map
var geocoder;                                              // object of class Geocoder
var markers = new Array();                                 // array with markers for all points on map
var searchMarker;                                          // marker for street searching
var imageMarker = "../../Images/marker.png";               // image for markers
var imageSearchMarker = "../../Images/newmarker.png";      // image for marker for street searching
var zoom;                                                  // map scale


function AlreadySetLocation() {                                          //function for specific coordinates of the map's center. If user came from action PointInfo
    var stringLocation = $("#stringforMap").data("str");
    if (stringLocation != "") {                                            //execute only if coordinates have been sent from action
        var arr = stringLocation.split("-");
        myLatlng = new google.maps.LatLng(arr[0], arr[1]);                // set specific coordinates
        zoom = 14;                                                        // set approximate scale for map
    }
    else
    {
        myLatlng = new google.maps.LatLng(48.466601, 35.018155);             //  set coordinates of the center of Dnipropetrovsk
        zoom = 13;                                                           //  set usual scale for map 
    }
        
}


function SetPoints() {                                                    //method to set all points on map
    var masPoints = document.getElementsByClassName("points");            // get coordiantes of all points from html
    if (masPoints.length > 0) {
        for (var x = 0; x < masPoints.length; x++) {
            var la = $(masPoints[x]).data('latitude');
            la = la.replace(",", ".");
            var ln = $(masPoints[x]).data('longitude');
            ln = ln.replace(",", ".");
            markers[x] = new google.maps.Marker({                    // create marker
                position: new google.maps.LatLng(la, ln),
                map: map,
                icon: imageMarker,
                title: $(masPoints[x]).data('adress')
            });
            markers[x].idPoint = $(masPoints[x]).data('id');                          // set marker's property with point's ID
            google.maps.event.addListener(markers[x], 'click', function () {         // sign marker on event "click"
                window.location.assign("../../Point/PointInfo/" + this.idPoint);     //   jump in the action of detailed view of point
            });
        }

        var markerClusterer = new MarkerClusterer(map, markers,                                 // create object MarkerClusterer to group markers on map
        {                                                                                               // settings to group markers
            maxZoom: 13,
            gridSize: 50,
            styles: null
        });

    }

}


function Initialize() {
    geocoder = new google.maps.Geocoder();      // create Geocoder object
    var mapOptions = {                          //  set options for map 
        center: myLatlng,
        zoom: zoom,
        mapTypeId: google.maps.MapTypeId.ROADMAP      // type of map. ROADMAP - for roads
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);           // create map
    SetPoints();                                                                            // call the method to set all points on map


    // Initialization of marker for search
    //02.04.2015 Konovalenko Anton
    searchMarker = new google.maps.Marker({
        map: map,
        icon: imageSearchMarker,
        draggable: true,
    });
    google.maps.event.addListener(searchMarker, 'click', function () {             // sign marker on event "click"
        var stringForMap = this.latOk + "-" + this.longOk;
        window.location.assign("../../Point/Add?stringForMap=" + stringForMap);   // path to the action of point creation
    });
}



//Autocomplete and positioning
//02.04.2015 Konovalenko Anton
$(function() {
    $("#searchAdress").autocomplete({
        //determine the value for adress of geocoding
        source: function (request, response) {
            var geocoderRequest = {
            //    'address': request.term,
                'componentRestrictions': {
                    'country': 'ua',
                    'locality': 'Днепропетровск',
                    'route': request.term
                },
            };

            geocoder.geocode(geocoderRequest, function (results, status) {
                response($.map(results, function(item) {
                    return {
                        FullAddress: item.formatted_address,
                        value: item.formatted_address,
                        latitude: item.geometry.location.lat(),
                        longitude: item.geometry.location.lng()
                    }
                }));
            })
        },

        // Execute when specific adress was selected
        select: function(event, ui) {
            var location = new google.maps.LatLng(ui.item.latitude, ui.item.longitude);
            map.setCenter(location);

            searchMarker.setPosition(location);
            searchMarker.latOk = ui.item.latitude;
            searchMarker.longOk = ui.item.longitude;
            searchMarker.setTitle(ui.item.FullAddress);
        }
    });
})